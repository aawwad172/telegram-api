using System.Data;
using Microsoft.Data.SqlClient;
using Telegram.API.Domain.Entities.User;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Infrastructure.Persistence.Repositories;

public class RecipientRepository(IDbConnectionFactory connectionFactory) : IRecipientRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    public async Task<Recipient?> GetRecipientAsync(string phoneNumber, int botId, CancellationToken cancellationToken = default)
    {
        using IDbConnection conn = await _connectionFactory.CreateOpenConnection();

        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_GetTelegramUser";

        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.NVarChar)
        { Value = phoneNumber }
        );

        cmd.Parameters.Add(new SqlParameter("@BotId", SqlDbType.Int)
        { Value = botId }
        );

        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Recipient
            {
                BotId = reader.GetInt32(reader.GetOrdinal("BotId")),
                ChatId = reader.GetString(reader.GetOrdinal("ChatId")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                CreationDateTime = reader.GetDateTime(reader.GetOrdinal("CreationDateTime")),
                Username = reader.IsDBNull(reader.GetOrdinal("Username")) ? null : reader.GetString(reader.GetOrdinal("Username")),
                FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? null : reader.GetString(reader.GetOrdinal("FirstName")),
                LastSeenDateTime = reader.GetDateTime(reader.GetOrdinal("LastSeenDateTime")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
        // Not found
        return null;
    }

    public async Task<IDictionary<string, string?>> GetChatIdsAsync(IEnumerable<string> phoneNumbers, int botId, CancellationToken cancellationToken = default)
    {
        // Deduplicate + materialize once
        List<string> list = phoneNumbers
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        // Build TVP
        DataTable tvp = new();
        tvp.Columns.Add("PhoneNumber", typeof(string));
        foreach (string? p in list)
            tvp.Rows.Add(p);

        using IDbConnection conn = await _connectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_GetChatIdsForPhones";

        cmd.Parameters.Add(new SqlParameter("@BotId", SqlDbType.Int) { Value = botId });

        SqlParameter phonesParam = new("@PhoneNumbers", SqlDbType.Structured)
        {
            TypeName = "dbo.PhoneList",
            Value = tvp
        };
        cmd.Parameters.Add(phonesParam);

        Dictionary<string, string?> map = new(list.Count, StringComparer.Ordinal);

        using SqlDataReader rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
        {
            string phone = rdr.GetString(0);                // PhoneNumber
            string? chat = rdr.IsDBNull(1) ? null : rdr.GetString(1); // ChatId
            map[phone] = chat;
        }

        // Ensure all requested phones appear in the map (null if not found)
        foreach (string? p in list)
            map.TryAdd(p, null);

        return map;
    }

    public Task DeactivateAsync(int botId, string chatId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<Recipient>> GetActiveChatsAsync(int botId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Recipient?> GetAsync(int botId, string chatId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

}
