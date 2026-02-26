using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MafiaPickem.Api.Data;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration["SqlConnectionString"]
            ?? throw new InvalidOperationException("SqlConnectionString not configured");
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
