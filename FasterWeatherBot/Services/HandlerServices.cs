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

        private const string WeatherCommand = "Weather";
        private const string LocationCommand = "Your saved location";
        private const string AddCommand = "Add location";

        // Dictionaries to track users waiting for input
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
                        await botClient.SendTextMessageAsync(chatId, "⚠ Enter the correct text");
                        return;
                    }

                    // If the user is waiting to enter a city
                    if (waitingForCity.TryGetValue(chatId, out bool isWaiting) && isWaiting)
                    {
                        waitingForCity.Remove(chatId);
                        string weatherInfo = await WeatherServices.GetWeatherAsync(text, chatId);
                        await botClient.SendTextMessageAsync(chatId, weatherInfo, parseMode: ParseMode.Markdown);
                        return;
                    }

                    // If the user is waiting to enter a place to save
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
                        case WeatherCommand:
                            waitingForCity[chatId] = true;
                            await botClient.SendTextMessageAsync(chatId, "📍 Enter location to get the weather:");
                            return;

                        case LocationCommand:
                            var savedPlaces = await SavedPlacesServices.GetAllSavedPlacesAsync();
                            var userSavedPlace = savedPlaces.FirstOrDefault(sp => sp.UserId == chatId);

                            if (userSavedPlace.SavedPlace != null)
                            {
                                string weatherInfo = await WeatherServices.GetWeatherAsync(userSavedPlace.SavedPlace, chatId);
                                await botClient.SendTextMessageAsync(chatId, weatherInfo, parseMode: ParseMode.Markdown);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(chatId, "❌ You don't have a saved location yet. Use 'Add location' to save one.");
                            }
                            return;

                        case AddCommand:
                            waitingForPlace[chatId] = true;
                            await botClient.SendTextMessageAsync(chatId, "➕ Enter the name of the location you want to add:");
                            return;
                        case "Login":
                            // Log in the user
                            string userName = user.Username ?? "Unknown";
                            string languageCode = user.LanguageCode ?? "uk";
                            bool isBot = user.IsBot;
                            await LoginUser.LoginUserAsync(botClient, chatId, userName, languageCode, isBot);

                            // Update keyboard after login
                            var loggedInKeyboard = new ReplyKeyboardMarkup(new[]
                            {
        new KeyboardButton[] { WeatherCommand },
        new KeyboardButton[] { LocationCommand },
        new KeyboardButton[] { AddCommand }
    })
                            {
                                ResizeKeyboard = true,
                                OneTimeKeyboard = false
                            };

                            await botClient.SendTextMessageAsync(chatId, "✅ You are successfully logged in!", replyMarkup: loggedInKeyboard);
                            return;

                        default:
                            // Default keyboard options (visible before login)
                            var defaultKeyboard = new ReplyKeyboardMarkup(new[]
                            {
        new KeyboardButton[] { WeatherCommand }
    })
                            {
                                ResizeKeyboard = true,
                                OneTimeKeyboard = false
                            };

                            // Add login button if user is not registered
                            if (!userExists)
                            {
                                defaultKeyboard.Keyboard = defaultKeyboard.Keyboard.Append(new KeyboardButton[] { "Login" }).ToArray();
                            }
                            else
                            {
                                // If the user is logged in, update the keyboard with additional options
                                defaultKeyboard.Keyboard = new[]
                                {
            new KeyboardButton[] { WeatherCommand },
            new KeyboardButton[] { LocationCommand },
            new KeyboardButton[] { AddCommand }
        };
                            }

                            await botClient.SendTextMessageAsync(
                                chatId,
                                userExists ? "Welcome back!" : "Welcome! Please login.",
                                replyMarkup: defaultKeyboard
                            );
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        // Handles bot errors
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
