using System.Data;
using Microsoft.Data.SqlClient;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Infrastructure.Persistence.Repositories;

public class BotRepository(IDbConnectionFactory dbConnectionFactory) : IBotRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

    public async Task<Bot?> GetByIdAsync(int botId, int customerId, CancellationToken cancellationToken = default)
    {
        using IDbConnection conn = await _dbConnectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_GetBotById";
        cmd.Parameters.Add(new SqlParameter("@BotId", SqlDbType.Int)
        { Value = botId }
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
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            EncryptedBotKey = reader.GetString(reader.GetOrdinal("EncryptedBotKey")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreationDateTime = reader.GetDateTime(reader.GetOrdinal("CreationDateTime")),
            WebhookSecret = reader.GetString(reader.GetOrdinal("WebhookSecret")),
            WebhookUrl = reader.GetString(reader.GetOrdinal("WebhookUrl")),
            PublicId = reader.GetString(reader.GetOrdinal("PublicId"))
        };
    }

    public async Task<Bot?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        using IDbConnection conn = await _dbConnectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_GetBotByPublicId";
        cmd.CommandTimeout = 30;

        // NVARCHAR(128) per schema
        cmd.Parameters.Add(new SqlParameter("@PublicId", SqlDbType.NVarChar, 128) { Value = publicId });

        using SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        // map
        return new Bot
        {
            BotId = reader.GetInt32(reader.GetOrdinal("BotId")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            EncryptedBotKey = reader.GetString(reader.GetOrdinal("EncryptedBotKey")),
            PublicId = reader.GetString(reader.GetOrdinal("PublicId")),
            WebhookSecret = reader.GetString(reader.GetOrdinal("WebhookSecret")),
            WebhookUrl = reader.GetString(reader.GetOrdinal("WebhookUrl")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreationDateTime = reader.GetDateTime(reader.GetOrdinal("CreationDateTime"))
        };
    }

    public async Task<Bot?> CreateAsync(Bot entity, CancellationToken ct = default)
    {
        using IDbConnection conn = await _dbConnectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_Bot_CreateBot";

        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = entity.CustomerId });

        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = entity.IsActive });

        cmd.Parameters.Add(new SqlParameter("@PublicId", SqlDbType.NVarChar, 128) { Value = entity.PublicId });

        cmd.Parameters.Add(new SqlParameter("@EncryptedBotKey", SqlDbType.NVarChar, 128) { Value = entity.EncryptedBotKey });

        cmd.Parameters.Add(new SqlParameter("@WebhookSecret", SqlDbType.NVarChar, 128) { Value = entity.WebhookSecret });

        cmd.Parameters.Add(new SqlParameter("@WebhookUrl", SqlDbType.NVarChar, 512) { Value = entity.WebhookUrl });

        using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return new Bot
            {
                BotId = reader.GetInt32(reader.GetOrdinal("BotId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                PublicId = reader.GetString(reader.GetOrdinal("PublicId")),
                EncryptedBotKey = reader.GetString(reader.GetOrdinal("EncryptedBotKey")),
                WebhookSecret = reader.GetString(reader.GetOrdinal("WebhookSecret")),
                WebhookUrl = reader.GetString(reader.GetOrdinal("WebhookUrl")),
                CreationDateTime = reader.GetDateTime(reader.GetOrdinal("CreationDateTime"))
            };
        }

        return null;
    }

    public async Task<bool> UpdateBotActivityAsync(int botId, bool isActive, CancellationToken cancellationToken = default)
    {
        using IDbConnection conn = await _dbConnectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_Bot_UpdateActivity";

        cmd.Parameters.Add(new SqlParameter("@BotId", SqlDbType.Int) { Value = botId });
        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = isActive });

        int rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0; // true if row updated
    }
}
