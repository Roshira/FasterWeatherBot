using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

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

    }
}
