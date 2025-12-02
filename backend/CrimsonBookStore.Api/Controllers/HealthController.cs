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

    [HttpPost("fix-orders")]
    public async Task<IActionResult> FixOrders()
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();
            
            // Delete child records first (POItem) due to foreign key constraint
            using var cmd1 = new MySqlCommand("DELETE FROM POItem WHERE POID = 0", conn);
            var deletedItems = await cmd1.ExecuteNonQueryAsync();
            
            // Then delete parent records (PurchaseOrder)
            using var cmd2 = new MySqlCommand("DELETE FROM PurchaseOrder WHERE POID = 0", conn);
            var deletedOrders = await cmd2.ExecuteNonQueryAsync();
            
            // Fix AUTO_INCREMENT to ensure it's set correctly
            // Get the current max POID
            using var cmd3 = new MySqlCommand("SELECT COALESCE(MAX(POID), 0) FROM PurchaseOrder", conn);
            var maxIdObj = await cmd3.ExecuteScalarAsync();
            var maxId = maxIdObj != null ? Convert.ToInt32(maxIdObj) : 0;
            
            // Set AUTO_INCREMENT to max + 1 (or 1 if no records exist)
            var nextId = maxId + 1;
            using var cmd4 = new MySqlCommand($"ALTER TABLE PurchaseOrder AUTO_INCREMENT = {nextId}", conn);
            await cmd4.ExecuteNonQueryAsync();
            
            return Ok(new
            {
                status = "fixed",
                deletedLineItems = deletedItems,
                deletedOrders = deletedOrders,
                maxPOID = maxId,
                nextAUTO_INCREMENT = nextId,
                message = "Cleaned up invalid purchase orders and reset AUTO_INCREMENT",
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

    [HttpGet("orders/{userId}")]
    public async Task<IActionResult> DebugOrders(int userId)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();
            
            // Get all orders for this user, including POID = 0
            using var cmd = new MySqlCommand("SELECT POID, UserID, Status, Total, UpdAt FROM PurchaseOrder WHERE UserID = @UserID ORDER BY UpdAt DESC", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            
            var orders = new List<object>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                orders.Add(new
                {
                    poid = reader.GetInt32(0),  // POID
                    userID = reader.GetInt32(1),  // UserID
                    status = reader.GetString(2),  // Status
                    total = reader.GetDecimal(3),  // Total
                    orderDate = reader.GetDateTime(4)  // UpdAt
                });
            }
            
            return Ok(new
            {
                userId = userId,
                orders = orders,
                count = orders.Count,
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

