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

        var Data = config["TelegramSettings:Database"];

        if (string.IsNullOrEmpty(Data))
        {
            Console.WriteLine("We haven't this appsettings.json.");
            return "error";
        }
        return Data;
    }
    public static string WeatherKeyLoader()
    {
        var config = LoadConfiguration();

        var WeatherKey = config["TelegramSettings:WeatherKey"];

        if (string.IsNullOrEmpty(WeatherKey))
        {
            Console.WriteLine("We haven't this appsettings.json.");
            return "error";
        }
        return WeatherKey;
    }
}