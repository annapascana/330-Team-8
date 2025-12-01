using CrimsonBookStore.Api.Data;
using CrimsonBookStore.Api.Models;
using Dapper;

namespace CrimsonBookStore.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM User WHERE Email = @Email",
            new { Email = email });
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM User WHERE UserID = @UserID",
            new { UserID = userId });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var conn = _connectionFactory.CreateConnection();
        // Search by FName + LName combination
        var nameParts = username.Split(' ');
        if (nameParts.Length >= 2)
        {
            return await conn.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM User WHERE FName = @FName AND LName = @LName",
                new { FName = nameParts[0], LName = string.Join(" ", nameParts.Skip(1)) });
        }
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM User WHERE FName = @Username OR LName = @Username",
            new { Username = username });
    }

    public async Task<List<User>> GetAllAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        var users = await conn.QueryAsync<User>("SELECT * FROM User ORDER BY CreatedAt DESC");
        return users.ToList();
    }

    public async Task<int> CreateAsync(User user)
    {
        using var conn = _connectionFactory.CreateConnection();
        
        // Get the next UserID by finding max and adding 1
        var maxId = await conn.QuerySingleOrDefaultAsync<int?>(
            "SELECT MAX(UserID) FROM User");
        var nextId = (maxId ?? 0) + 1;
        
        // Insert with explicit UserID
        var sql = @"INSERT INTO User (UserID, FName, LName, Email, PwdHash, Role, IsActive, CreatedAt)
                    VALUES (@UserID, @FName, @LName, @Email, @PwdHash, @Role, @IsActive, @CreatedAt)";
        user.UserID = nextId;
        await conn.ExecuteAsync(sql, user);
        
        return nextId;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"UPDATE User SET FName = @FName, LName = @LName, Email = @Email, Role = @Role, IsActive = @IsActive
                    WHERE UserID = @UserID";
        var rowsAffected = await conn.ExecuteAsync(sql, user);
        return rowsAffected > 0;
    }
}

