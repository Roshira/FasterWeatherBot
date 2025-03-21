using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Telegram.Bot;

public class ConfigService
{
    public static IConfigurationRoot LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        return builder.Build();
    }
    public static string DataBaseLoader()
    {
        var config = LoadConfiguration();

        var data = config["TelegramSettings:Database"];

        if (string.IsNullOrEmpty(data))
        {
            Console.WriteLine("We haven't this appsettings.json.");
            return "error";
        }
        return data;
    }
    public static string WeatherKeyLoader()
    {
        var config = LoadConfiguration();

        var weatherKey = config["TelegramSettings:WeatherKey"];

        if (string.IsNullOrEmpty(weatherKey))
        {
            Console.WriteLine("We haven't this appsettings.json.");
            return "error";
        }
        return weatherKey;
    }
}