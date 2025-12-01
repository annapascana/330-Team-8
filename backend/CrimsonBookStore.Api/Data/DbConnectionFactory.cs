using MySql.Data.MySqlClient;

namespace CrimsonBookStore.Api.Data;

public interface IDbConnectionFactory
{
    MySqlConnection CreateConnection();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        // Convert MySQL URI format to connection string
        if (connectionString.StartsWith("mysql://"))
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');
            _connectionString = $"Server={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Uid={userInfo[0]};Pwd={userInfo[1]};";
        }
        else
        {
            _connectionString = connectionString;
        }
    }

    public MySqlConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}

