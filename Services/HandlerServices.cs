using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using FasterWeatherBot.Models;

namespace FasterWeatherBot.Services
{
    internal class HandlerServices
    {
        public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {

                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            var message = update.Message;

                            var user = message.From;

                            Console.WriteLine($"{user.FirstName} ({user.Id}) Writed massege: {message.Text}");

                            var chat = message.Chat;

                            switch (message.Type)
                            {
                                case MessageType.Text:
                                    {
                                        var inlineKeyboard = new InlineKeyboardMarkup(
                                            new List<InlineKeyboardButton[]>()
                                            {


                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithUrl("Weather", "https://habr.com/"),
                                            InlineKeyboardButton.WithCallbackData("Your saved place", "button1"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Add place", "button2"),
                                            InlineKeyboardButton.WithCallbackData("Login", "button3"),
                                        },
                                            });
                                        await botClient.SendTextMessageAsync(
                                       chat.Id,
                                       "Это inline клавиатура!",
                                       replyMarkup: inlineKeyboard);
                                        return;
                                    }
                                default:
                                    {
                                        await botClient.SendTextMessageAsync(chat.Id, "Write only text");
                                        return;
                                    }
                                    return;
                            }
                        }
                    case UpdateType.CallbackQuery:
                        {
                            var callbackQuery = update.CallbackQuery;
                            var user = callbackQuery.From;
                            var chat = callbackQuery.Message.Chat;

                            Console.WriteLine($"{user.FirstName} ({user.Id}) натиснув кнопку: {callbackQuery.Data}");

                            switch (callbackQuery.Data)
                            {
                                case "button1":
                                    {
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                        await botClient.SendTextMessageAsync(chat.Id, $"Ви натиснули на {callbackQuery.Data}");


                                        return;
                                    }

                                case "button2":
                                    {
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "");
                                        await botClient.SendTextMessageAsync(chat.Id, $" {callbackQuery.Data}");


                                        return;
                                    }

                                case "button3":
                                    {
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "А це повноекранне повідомлення!", showAlert: true);
                                        await botClient.SendTextMessageAsync(chat.Id, $"Ви натиснули на {callbackQuery.Data}");

                                        // Отримуємо дані користувача
                                        string userName = callbackQuery.From.Username ?? "Unknown";
                                        string languageCode = callbackQuery.From.LanguageCode ?? "uk";
                                        bool isBot = callbackQuery.From.IsBot;

                                        // Викликаємо метод авторизації
                                        await LoginUserAsync(botClient, chat.Id, userName, languageCode, isBot);

                                        return;
                                    }
                            }

                            return;
                        }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        private static async Task LoginUserAsync(ITelegramBotClient botClient, long chatId, string userName, string languageCode, bool isBot)
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=FasterWeatherBot;Trusted_Connection=True;";
            var loginService = new LoginUser(connectionString);

            bool isLoggedIn = await loginService.Login(chatId, userName, languageCode, isBot);

            if (isLoggedIn)
            {
                await botClient.SendTextMessageAsync(chatId, "✅ Ви успішно авторизовані!");
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "⚠ Помилка авторизації. Спробуйте ще раз.");
            }
        }
    }
}
