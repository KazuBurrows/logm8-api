using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("SqlConnectionString")
            ?? configuration["SqlConnectionString"]
            ?? throw new InvalidOperationException("SQL connection string not configured");
    }

    public SqlConnection Create() => new SqlConnection(_connectionString);
}
