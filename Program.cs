using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using FasterWeatherBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

class Program
{
    private static ITelegramBotClient _botClient;

    private static ReceiverOptions _receiverOptions;

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var botToken = new TelegramBotClient("7918056211:AAGd-cQQDTd2gnfUlJtTZF5jzeflRP3At3w");

        // Додаємо його у DI-контейнер
        builder.Services.AddSingleton<ITelegramBotClient>(_ => botToken);
        builder.Services.AddControllers();

        // Додаємо Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "FasterWeatherBot API", Version = "v1" });
        });

        var app = builder.Build();

        // Включаємо Swagger тільки в режимі розробки
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FasterWeatherBot API v1");
                c.RoutePrefix = string.Empty; // Відкриває Swagger на головній сторінці
            });
        }

        app.UseAuthorization();
        app.MapControllers();

        // Запускаємо бота
        var botClient = app.Services.GetRequiredService<ITelegramBotClient>();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
        };

        using var cts = new CancellationTokenSource();
        botClient.StartReceiving(HandlerServices.UpdateHandler, HandlerServices.ErrorHandler, receiverOptions, cts.Token);

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"{me.FirstName} запущен!");

        await app.RunAsync();
    }
}