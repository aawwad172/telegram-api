using System.Data;
using Microsoft.Data.SqlClient;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Infrastructure.Persistence.Repositories;

public class BotRepository(IDbConnectionFactory dbConnectionFactory) : IBotRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

    public Task<Bot> CreateAsync(Bot entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Bot?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Bot>> ListAsync(int skip = 0, int take = 100, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, Bot entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Bot?> GetBotByKeyAsync(string EncryptedBotKey, int customerId)
    {
        using IDbConnection conn = await _dbConnectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_GetBotByKey";
        cmd.Parameters.Add(new SqlParameter("@EncryptedBotKey", SqlDbType.NVarChar, 128)
        { Value = EncryptedBotKey }
        );

        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int)
        { Value = customerId }
        );

        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        cmd.CommandTimeout = 30;
        if (!await reader.ReadAsync())
            return null; // Not Found

        return new Bot
        {
            BotId = reader.GetInt32(reader.GetOrdinal("BotId")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustId")),
            EncryptedBotKey = reader.GetString(reader.GetOrdinal("EncryptedBotKey")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreationDateTime = reader.GetDateTime(reader.GetOrdinal("CreationDateTime")),
            WebhookSecret = reader.GetString(reader.GetOrdinal("WebhookSecret")),
            WebhookUrl = reader.GetString(reader.GetOrdinal("WebhookUrl"))
        };
    }
}
