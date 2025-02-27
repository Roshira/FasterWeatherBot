using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using FasterWeatherBot.Services;

class Program
{
  private static ITelegramBotClient _botClient;

  private static ReceiverOptions _receiverOptions;

    private 

    static async Task Main()
    {

        _botClient = new TelegramBotClient("7918056211:AAGd-cQQDTd2gnfUlJtTZF5jzeflRP3At3w");
        _receiverOptions = new ReceiverOptions 
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message, 
                UpdateType.CallbackQuery // Inline buttons
            }
        };

        using var cts = new CancellationTokenSource();

        _botClient.StartReceiving(HandlerServices.UpdateHandler, HandlerServices.ErrorHandler, _receiverOptions, cts.Token); // Start bot

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"{me.FirstName} запущен!");

        await Task.Delay(-1); 
    }
}