using System.Data;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Microsoft.Data.SqlClient;

namespace Telegram.API.Infrastructure.Persistence;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connString;
    public DbConnectionFactory(string connString)
        => _connString = connString;

    public async Task<IDbConnection> CreateOpenConnection()
    {
        var conn = new SqlConnection(_connString);
        await conn.OpenAsync().ConfigureAwait(false);
        return conn;
    }
}
