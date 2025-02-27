using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Telegram.Bot;

namespace FasterWeatherBot.Models
{
    internal class LoginUser
    {
        private readonly string _connectionString;

        public LoginUser(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> Login(long userId, string userName, string languageCode, bool isBot)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var user = await db.QueryFirstOrDefaultAsync<Users>(
    "SELECT * FROM Users WHERE Id = @Id OR UserName = @UserName", new { Id = userId, UserName = userName });

                if (user == null)
                {
                    string insertQuery = "INSERT INTO Users (Id, UserName, LanguageCode, IsBot) VALUES (@Id, @UserName, @LanguageCode, @IsBot)";
                    int rowsAffected = await db.ExecuteAsync(insertQuery, new
                    {
                        Id = userId,
                        UserName = userName,
                        LanguageCode = languageCode,
                        IsBot = isBot
                    });

                    return rowsAffected > 0;
                }

                return false;
            }
        }
        internal static async Task<bool> CheckIfUserExists(long userId)
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=FasterWeatherBot;Trusted_Connection=True;";
            using (var db = new SqlConnection(connectionString))
            {
                var user = await db.QueryFirstOrDefaultAsync<Users>(
                    "SELECT * FROM Users WHERE Id = @Id", new { Id = userId });

                return user != null;
            }
        }

        internal static async Task LoginUserAsync(ITelegramBotClient botClient, long chatId, string userName, string languageCode, bool isBot)
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=FasterWeatherBot;Trusted_Connection=True;";
            var loginService = new LoginUser(connectionString);

            bool isLoggedIn = await loginService.Login(chatId, userName, languageCode, isBot);

            if (!isLoggedIn)
            {
                await botClient.SendTextMessageAsync(chatId, "⚠ Authorization error. Try again.");
            }
        }
    }
}
