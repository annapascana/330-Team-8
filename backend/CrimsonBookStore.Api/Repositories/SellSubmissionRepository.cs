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
        await conn.OpenAsync();
        
        using var transaction = await conn.BeginTransactionAsync();
        
        try
        {
            // Get the next SubID (since SubID is not AUTO_INCREMENT, we need to generate it manually)
            var maxId = await conn.QuerySingleAsync<int>("SELECT COALESCE(MAX(SubID), 0) FROM SellSub", transaction: transaction);
            var submissionId = maxId + 1;
            
            // Condition is a reserved keyword in MySQL, so we need to escape it with backticks
            var sql = @"INSERT INTO SellSub (SubID, UserID, Title, AuthTxt, ISBN, Edition, `Condition`, 
                        AskPrice, Status, CreatedAt)
                        VALUES (@SubID, @UserID, @Title, @AuthTxt, @ISBN, @Edition, @Condition,
                        @AskPrice, @Status, @CreatedAt)";
            
            var parameters = new
            {
                SubID = submissionId,
                UserID = submission.UserID,
                Title = submission.Title,
                AuthTxt = submission.AuthTxt,
                ISBN = submission.ISBN,
                Edition = submission.Edition,
                Condition = submission.Condition,
                AskPrice = submission.AskPrice,
                Status = submission.Status,
                CreatedAt = submission.CreatedAt
            };
            
            var rowsAffected = await conn.ExecuteAsync(sql, parameters, transaction: transaction);
            
            if (rowsAffected == 0)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to create sell submission");
            }
            
            await transaction.CommitAsync();
            return submissionId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(SellSubmission submission)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"UPDATE SellSub SET Status = @Status, 
                    ReviewedAt = @ReviewedAt WHERE SubID = @SubID";
        var rowsAffected = await conn.ExecuteAsync(sql, submission);
        return rowsAffected > 0;
    }

    public async Task<int> CountApprovedByUserAndISBNAsync(int userId, string isbn)
    {
        using var conn = _connectionFactory.CreateConnection();
        var count = await conn.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM SellSub WHERE UserID = @UserID AND ISBN = @ISBN AND Status = 'Approved'",
            new { UserID = userId, ISBN = isbn });
        return count;
    }
}

