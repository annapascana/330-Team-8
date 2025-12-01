using CrimsonBookStore.Api.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace CrimsonBookStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;

    public HealthController(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [HttpGet("database")]
    public async Task<IActionResult> TestDatabase()
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();
            
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM User", conn);
            var userCount = await cmd.ExecuteScalarAsync();
            
            using var cmd2 = new MySqlCommand("SELECT COUNT(*) FROM Book", conn);
            var bookCount = await cmd2.ExecuteScalarAsync();
            
            return Ok(new
            {
                status = "connected",
                database = conn.Database,
                userCount = userCount,
                bookCount = bookCount,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

