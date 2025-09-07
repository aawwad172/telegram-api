using Microsoft.Data.SqlClient;
using System.Data;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Infrastructure.Persistence.Repositories;

public class CustomerRepository(IDbConnectionFactory connectionFactory) : ICustomerRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public Task<Customer> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<Customer?> GetCustomerByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        using IDbConnection conn = await _connectionFactory.CreateOpenConnection();

        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_GetCustomerByUsername";
        cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar)
        { Value = username }
        );

        using SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (await reader.ReadAsync())
        {
            return new Customer
            {
                // This is the only column that is named CustId since it is linked to an old table and we can't change it's name :)
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("Password")),
                RequireSystemApprove = reader.GetBoolean(reader.GetOrdinal("RequireSystemApprove")),
                RequireAdminApprove = reader.GetBoolean(reader.GetOrdinal("RequireAdminApprove")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                IsBlocked = reader.GetBoolean(reader.GetOrdinal("IsBlocked")),
                IsTelegramActive = reader.GetBoolean(reader.GetOrdinal("IsTelegramActive"))
            };
        }
        return null;
    }
}
