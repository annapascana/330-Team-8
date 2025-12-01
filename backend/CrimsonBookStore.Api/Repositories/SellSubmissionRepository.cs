using CrimsonBookStore.Api.Data;
using CrimsonBookStore.Api.Models;
using Dapper;

namespace CrimsonBookStore.Api.Repositories;

public class SellSubmissionRepository : ISellSubmissionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SellSubmissionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<SellSubmission>> GetByUserIdAsync(int userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var submissions = await conn.QueryAsync<SellSubmission>(
            "SELECT * FROM SellSub WHERE UserID = @UserID ORDER BY CreatedAt DESC",
            new { UserID = userId });
        return submissions.ToList();
    }

    public async Task<List<SellSubmission>> GetAllAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        var submissions = await conn.QueryAsync<SellSubmission>(
            "SELECT * FROM SellSub ORDER BY CreatedAt DESC");
        return submissions.ToList();
    }

    public async Task<SellSubmission?> GetByIdAsync(int submissionId)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<SellSubmission>(
            "SELECT * FROM SellSub WHERE SubID = @SubID",
            new { SubID = submissionId });
    }

    public async Task<int> CreateAsync(SellSubmission submission)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO SellSub (UserID, Title, AuthTxt, ISBN, Edition, Condition, 
                    AskPrice, Status, CreatedAt)
                    VALUES (@UserID, @Title, @AuthTxt, @ISBN, @Edition, @Condition,
                    @AskPrice, @Status, @CreatedAt);
                    SELECT LAST_INSERT_ID();";
        return await conn.QuerySingleAsync<int>(sql, submission);
    }

    public async Task<bool> UpdateAsync(SellSubmission submission)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"UPDATE SellSub SET Status = @Status, 
                    ReviewedAt = @ReviewedAt WHERE SubID = @SubID";
        var rowsAffected = await conn.ExecuteAsync(sql, submission);
        return rowsAffected > 0;
    }
}

