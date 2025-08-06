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
