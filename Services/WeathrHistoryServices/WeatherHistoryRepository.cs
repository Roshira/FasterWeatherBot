using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace FasterWeatherBot.Models
{
    public class WeatherHistoryRepository
    {
        private readonly string _connectionString;

        public WeatherHistoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void SaveWeatherHistory(WeatherHistoryDate weather)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO WeatherHistory (UserId, City, Temperature, Humidity, Description, TimeString)
                                 VALUES (@UserId, @City, @Temperature, @Humidity, @Description, @TimeString)";

                db.Execute(query, weather);
            }
        }
    }
}
