using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using FasterWeatherBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Reflection;

class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;

    public static async Task Main(string[] args)
    {
        // Create a WebApplication builder
        var builder = WebApplication.CreateBuilder(args);

        // Initialize Telegram bot client with the given token
        var botToken = new TelegramBotClient("7918056211:AAGd-cQQDTd2gnfUlJtTZF5jzeflRP3At3w");

        // Register the bot client in the DI container
        builder.Services.AddSingleton<ITelegramBotClient>(_ => botToken);
        builder.Services.AddControllers();

        // Add Swagger for API documentation
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "FasterWeatherBot API", Version = "v1" });

            // Include XML documentation (if available)
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        // Build the application
        var app = builder.Build();

        // Enable Swagger UI
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FasterWeatherBot API v1");
            c.RoutePrefix = string.Empty; // Set Swagger UI as the default page
        });

        // Enable authorization (not configured yet)
        app.UseAuthorization();
        app.MapControllers(); // Map controller routes

        // Start the bot
        var botClient = app.Services.GetRequiredService<ITelegramBotClient>();

        // Define receiver options to specify allowed update types
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
        };

        using var cts = new CancellationTokenSource();

        // Start receiving updates and handle them with the provided methods
        botClient.StartReceiving(HandlerServices.UpdateHandler, HandlerServices.ErrorHandler, receiverOptions, cts.Token);

        // Get bot information and display its name in the console
        var me = await botClient.GetMeAsync();
        Console.WriteLine($"{me.FirstName} start!");

        // Run the web application
        await app.RunAsync();
    }
}
