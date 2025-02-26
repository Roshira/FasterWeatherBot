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
using Microsoft.AspNetCore.SignalR;

namespace FasterWeatherBot.Services
{
    internal class HandlerServices
    {

        private static Dictionary<long, bool> waitingForCity = new Dictionary<long, bool>();
        public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {

                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            var message = update.Message;
                            long chatId = message.Chat.Id;

                            if (waitingForCity.ContainsKey(chatId) && waitingForCity[chatId])
                            {
                                string city = message.Text.Trim();

                                if (string.IsNullOrEmpty(city))
                                {
                                    await botClient.SendTextMessageAsync(chatId, "⚠ Enter the correct city name.");
                                    return;
                                }

                                string weatherInfo = await WeatherServices.GetWeatherAsync(city, chatId);

                                await botClient.SendTextMessageAsync(chatId, weatherInfo, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

                                waitingForCity.Remove(chatId);
                                return;
                            }

                            var replyKeyboard = new ReplyKeyboardMarkup(new[]
                            {
        new KeyboardButton("Start")
    })
                            {
                                ResizeKeyboard = true,
                                OneTimeKeyboard = false 
                            };

                            var user = message.From;
                            var userId = user.Id;

                            bool userExists = await LoginUser.CheckIfUserExists(userId);

                            var inlineKeyboard = new InlineKeyboardMarkup(
                                new List<InlineKeyboardButton[]>()
                                {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Weather", "buttonWeather"),
                    InlineKeyboardButton.WithCallbackData("Your saved place", "button1"),
                },
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Add place", "button2"),
                    userExists ? null : InlineKeyboardButton.WithCallbackData("Login", "buttonLogin"),
                }.Where(x => x != null).ToArray(),
                                });

                            // Відправляємо повідомлення з кнопками
                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                userExists ? "Welcome back!" : "Welcome back! Please log in.",
                                replyMarkup: inlineKeyboard
                            );

                            return;
                        }

                    case UpdateType.CallbackQuery:
                        {
                            var callbackQuery = update.CallbackQuery;
                            var user = callbackQuery.From;
                            var chat = callbackQuery.Message.Chat;

                            Console.WriteLine($"{user.FirstName} ({user.Id}) Click button: {callbackQuery.Data}");

                            switch (callbackQuery.Data)
                            {
                                 
                                case "buttonWeather":
                                    {
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                        waitingForCity[chat.Id] = true;
                                        await botClient.SendTextMessageAsync(chat.Id, "📍 Enter the name of the city for which you want to get the weather:");
                                        return;
                                    }
                                case "button1":
                                    {
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);


                                        return;
                                    }

                                case "button2":
                                    {
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "");


                                        return;
                                    }

                                case "buttonLogin":
                                    {
                                        string userName = callbackQuery.From.Username ?? "Unknown";
                                        string languageCode = callbackQuery.From.LanguageCode ?? "uk";
                                        bool isBot = callbackQuery.From.IsBot;

                                        await LoginUser.LoginUserAsync(botClient, chat.Id, userName, languageCode, isBot);

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
       
    }
}
