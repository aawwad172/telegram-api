using System.Data;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Telegram.API.Domain.Settings;

namespace Telegram.API.Infrastructure.Persistence;

public class DbConnectionFactory(IOptionsMonitor<DbSettings> options) : IDbConnectionFactory
{
    private readonly IOptionsMonitor<DbSettings> _options = options;

    public async Task<IDbConnection> CreateOpenConnection()
    {
        SqlConnection conn = new(_options.CurrentValue.ConStr);
        await conn.OpenAsync().ConfigureAwait(false);
        return conn;
    }
}
