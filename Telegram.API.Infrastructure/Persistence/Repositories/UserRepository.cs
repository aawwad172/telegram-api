using Microsoft.Data.SqlClient;
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

    public async Task<User> GetByUsernameAsync(string username)
    {
        var conn = await _connectionFactory.CreateOpenConnection();

        using var cmd = (SqlCommand)conn.CreateCommand();
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.CommandText = "usp_GetUserByUsername";
        cmd.Parameters.Add(new SqlParameter("@Username", System.Data.SqlDbType.NVarChar)
        { Value = username }
        );

        SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (reader.Read())
        {
            return new User
            {
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("Password")),
            };
        }
        else
        {
            throw new NotFoundException($"User with username '{username}' not found.");
        }
    }
}
