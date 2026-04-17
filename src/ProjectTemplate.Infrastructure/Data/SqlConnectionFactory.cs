using System.Data;
using Npgsql;
using ProjectTemplate.Application.Abstractions.Data;

namespace ProjectTemplate.Infrastructure.Data;

internal sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}
