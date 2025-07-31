using Microsoft.Data.SqlClient;
using System.Data;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
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

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using IDbConnection conn = await _connectionFactory.CreateOpenConnection();

        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.CommandText = "usp_GetUserByUsername";
        cmd.Parameters.Add(new SqlParameter("@Username", System.Data.SqlDbType.NVarChar)
        { Value = username }
        );

        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("Password")),
                RequirSystemApprove = reader.GetBoolean(reader.GetOrdinal("RequireSystemApprove")),
                RequireAdminApprove = reader.GetBoolean(reader.GetOrdinal("RequireAdminApprove")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                IsBlocked = reader.GetBoolean(reader.GetOrdinal("IsBlocked")),
                IsTelegramActive = reader.GetBoolean(reader.GetOrdinal("IsTelegramActive"))
            };
        }
        else
        {
            return null;
        }
    }
}
