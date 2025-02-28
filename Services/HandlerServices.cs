using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using FasterWeatherBot.Models;
using Telegram.Bot.Exceptions;

namespace FasterWeatherBot.Services
{
    internal class HandlerServices
    {
        private static Dictionary<long, bool> waitingForCity = new Dictionary<long, bool>();
        private static Dictionary<long, bool> waitingForPlace = new Dictionary<long, bool>();

        public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message is not null)
                {
                    var message = update.Message;
                    var chatId = message.Chat.Id;
                    var text = message.Text?.Trim();

                    if (string.IsNullOrEmpty(text))
                    {
                        await botClient.SendTextMessageAsync(chatId, "⚠Enter the correct text");
                        return;
                    }

                    // Перевіряємо, чи користувач очікує введення міста
                    if (waitingForCity.TryGetValue(chatId, out bool isWaiting) && isWaiting)
                    {
                        waitingForCity.Remove(chatId); // Видаляємо з очікування

                        string weatherInfo = await WeatherServices.GetWeatherAsync(text, chatId);
                        await botClient.SendTextMessageAsync(chatId, weatherInfo, parseMode: ParseMode.Markdown);
                        return;
                    }

                    if (waitingForPlace.TryGetValue(chatId, out bool isWaitingPlace) && isWaitingPlace)
                    {
                        waitingForPlace.Remove(chatId);
                        await SavedPlacesServices.SaveOrUpdatePlaceAsync(chatId, text);
                        await botClient.SendTextMessageAsync(chatId, $"✅ '{text}' saved successfully!");
                        return;
                    }


                    var user = message.From;
                    var userId = user.Id;
                    bool userExists = await LoginUser.CheckIfUserExists(userId);

                    switch (text)
                    {
                        case "Weather":
                            waitingForCity[chatId] = true;
                            await botClient.SendTextMessageAsync(chatId, "📍Enter location to get the weather:");
                            return;

                        case "Your saved location":
                            var savedPlaces = await SavedPlacesServices.GetAllSavedPlacesAsync();
                            var userSavedPlace = savedPlaces.FirstOrDefault(sp => sp.UserId == chatId);

                            if (userSavedPlace.SavedPlace != null)
                            {
                                string weatherInfo = await WeatherServices.GetWeatherAsync(userSavedPlace.SavedPlace, chatId);
                                await botClient.SendTextMessageAsync(chatId, weatherInfo, parseMode: ParseMode.Markdown);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(chatId, "❌ You don't have saved location yet. Use 'Add place' to save one.");
                            }
                            return;

                        case "Add location":
                            waitingForPlace[chatId] = true;
                            await botClient.SendTextMessageAsync(chatId, "➕ Enter the name of the location you want to add:");
                            return;

                        case "Login":
                            string userName = user.Username ?? "Unknown";
                            string languageCode = user.LanguageCode ?? "uk";
                            bool isBot = user.IsBot;
                            await LoginUser.LoginUserAsync(botClient, chatId, userName, languageCode, isBot);

                            // Оновлюємо панель після логіну
                            var newKeyboard = new ReplyKeyboardMarkup(new[]
                            {
                                new KeyboardButton[] { "Weather" },
                                new KeyboardButton[] { "Your saved location" },
                                new KeyboardButton[] { "Add location" }
                            })
                            {
                                ResizeKeyboard = true,
                                OneTimeKeyboard = false
                            };
                            var tempMessage = await botClient.SendTextMessageAsync(
                             chatId,
                            "✅You are successfully logged in!"
                             );
                            await botClient.SendTextMessageAsync(
           chatId,
           "",
           replyMarkup: newKeyboard
       );
                            return;

                        default:
                            var replyKeyboard = new ReplyKeyboardMarkup(new[]
                            {
                                new KeyboardButton[] { "Weather" },
                                new KeyboardButton[] { "Your saved location" },
                                new KeyboardButton[] { "Add location" }
                            })
                            {
                                ResizeKeyboard = true,
                                OneTimeKeyboard = false
                            };

                            if (!userExists)
                            {
                                replyKeyboard.Keyboard = replyKeyboard.Keyboard.Append(new KeyboardButton[] { "Login" }).ToArray();
                            }

                            await botClient.SendTextMessageAsync(
                                chatId,
                                userExists ? "Welcome back!" : "Welcome! Please login.",
                                replyMarkup: replyKeyboard
                            );
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex}");
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
