using System;
using System.Data.SqlClient;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Dapper;
using FasterWeatherBot.Models;

public class WeatherServices
{
    private static readonly string apiUrl = "https://api.openweathermap.org/data/2.5/weather?q={0}&appid={1}&units=metric&lang=en";



    public static async Task<string> GetWeatherAsync(string city, long UserId)
    {

        try
        {
            using (HttpClient client = new HttpClient())
            {

                // Send request to the weather API
                string requestUrl = string.Format(apiUrl, city, ConfigService.WeatherKeyLoader());
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return "❌ Failed to get weather data. Check the correctness of the location name.";
                }

                // Parse JSON response
                string responseData = await response.Content.ReadAsStringAsync();
                JObject weatherData = JObject.Parse(responseData);

                string description = weatherData["weather"][0]["description"].ToString();
                string temperature = weatherData["main"]["temp"].ToString();
                string feelsLike = weatherData["main"]["feels_like"].ToString();
                string humidity = weatherData["main"]["humidity"].ToString();

                // Save weather data to database
                DateTime executionTime = DateTime.Now;
                string timeString = executionTime.ToString("yyyy-MM-dd HH:mm:ss");
                string connectionString = ConfigService.DataBaseLoader();

                var repository = new WeatherHistoryRepository(connectionString);

                var weatherHistory = new WeatherHistoryDate
                {
                    UserId = UserId,
                    City = city,
                    Temperature = $"{temperature}°C",
                    Humidity = $"{humidity}%",
                    Description = description,
                    TimeString = timeString
                };

                repository.SaveWeatherHistory(weatherHistory);
                Console.WriteLine("Data saved!");

                // Return formatted weather info
                return $"🌦 Weather in {city}:\n" +
                       $"🌡 Temperature: {temperature}°C (Feels like {feelsLike}°C)\n" +
                       $"💧 Humidity: {humidity}%\n" +
                       $"📌 Description: {description}";
            }
        }
        catch (Exception ex)
        {
            return "⚠ An error occurred while receiving weather data.";
        }
    }
}
