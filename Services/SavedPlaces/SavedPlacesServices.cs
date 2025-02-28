using Dapper;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FasterWeatherBot.Services
{
    internal class SavedPlacesServices
    {
        // Database connection string
        private static readonly string connectionString = "Server=(localdb)\\mssqllocaldb;Database=FasterWeatherBot;Trusted_Connection=True;";

        // Save or update a user's saved place
        public static async Task SaveOrUpdatePlaceAsync(long userId, string place)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"
                MERGE INTO SavedPlaces AS target
                USING (SELECT @UserId AS IdUser, @Place AS SavedPlace) AS source
                ON target.IdUser = source.IdUser
                WHEN MATCHED THEN 
                    UPDATE SET target.SavedPlace = source.SavedPlace
                WHEN NOT MATCHED THEN
                    INSERT (IdUser, SavedPlace) VALUES (source.IdUser, source.SavedPlace);";

            await connection.ExecuteAsync(query, new { UserId = userId, Place = place });
        }

        // Retrieve all saved places from the database
        public static async Task<IEnumerable<(long UserId, string SavedPlace)>> GetAllSavedPlacesAsync()
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT IdUser, SavedPlace FROM SavedPlaces";
            return await connection.QueryAsync<(long, string)>(query);
        }
    }
}
