using System.Data;
using API.Template.Domain.Interfaces.Infrastructure;
using Microsoft.Data.SqlClient;

namespace API.Template.Infrastructure.Persistence;

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
