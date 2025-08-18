using Microsoft.Data.SqlClient;
using System.Data;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Infrastructure.Persistence.Repositories;

public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    public Task<User> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IDictionary<string, string?>> GetChatIdsAsync(IEnumerable<string> phoneNumbers, string botKey)
    {
        // Deduplicate + materialize once
        List<string> list = phoneNumbers
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.Ordinal)
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

        cmd.Parameters.Add(new SqlParameter("@BotKey", SqlDbType.NVarChar, 100) { Value = botKey });

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

    public async Task<User?> GetUserAsync(string phoneNumber, string botKey)
    {
        using IDbConnection conn = await _connectionFactory.CreateOpenConnection();

        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_GetTelegramUser";

        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.NVarChar)
        { Value = phoneNumber }
        );

        cmd.Parameters.Add(new SqlParameter("@BotKey", SqlDbType.NVarChar)
        { Value = botKey }
        );

        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                ChatId = reader.GetString(reader.GetOrdinal("ChatId")),
                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                BotKey = reader.GetString(reader.GetOrdinal("BotKey")),
                CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"))
            };
        }
        else
        {
            return null;
        }
    }
}
