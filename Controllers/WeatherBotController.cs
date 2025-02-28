using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data.SqlClient;
using FasterWeatherBot.Models;
using Telegram.Bot;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


[ApiController]
[Route("api/weatherbot")]
public class WeatherBotController : ControllerBase
{
    private readonly string _connectionString = "Server=(localdb)\\mssqllocaldb;Database=FasterWeatherBot;Trusted_Connection=True;";
    private readonly ITelegramBotClient _botClient;

    public WeatherBotController(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserHistory(long userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var user = await connection.QueryFirstOrDefaultAsync<Users>("SELECT * FROM Users WHERE Id = @Id", new { Id = userId });
        if (user == null) return NotFound("User not found");

        var history = await connection.QueryAsync<WeatherHistoryDate>("SELECT * FROM WeatherHistory WHERE UserId = @UserId", new { UserId = userId });

        return Ok(new { user, history });
    }

    [HttpPost("send-weather")]
    public async Task<IActionResult> SendWeather([FromQuery] long? userId, [FromQuery] string city)
    {
        if (string.IsNullOrEmpty(city))
        {
            return BadRequest("City is required");
        }

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        List<Users> users;
        if (userId.HasValue)
        {
            var user = await connection.QueryFirstOrDefaultAsync<Users>("SELECT * FROM Users WHERE Id = @Id", new { Id = userId });
            if (user == null) return NotFound("User not found");
            users = new List<Users> { user };
        }
        else
        {
            users = (await connection.QueryAsync<Users>("SELECT * FROM Users")).ToList();
        }

        try
        {
            string weatherInfo = await WeatherServices.GetWeatherAsync(city, 0);
            foreach (var user in users)
            {
                await _botClient.SendTextMessageAsync(user.Id, weatherInfo);
            }

            return Ok("Weather sent successfully");
        }
        catch (Exception ex)
        {
            // Логируйте ошибку
            return StatusCode(500, "Internal server error");
        }
    }
}