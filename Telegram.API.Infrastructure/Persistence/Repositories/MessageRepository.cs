using Microsoft.Data.SqlClient;
using System.Data;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Infrastructure.Persistence.Repositories;

public class MessageRepository(IDbConnectionFactory connectionFactory) : IMessageRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    /// <summary>
    ///     Getting the chat ID for a given phone number and bot key.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="botKey"></param>
    /// <returns>ChatId if the user is supscribed</returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<string> GetChatId(string phoneNumber, string botKey)
    {
        IDbConnection conn = await _connectionFactory.CreateOpenConnection();

        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_GetChatId";

        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.NVarChar)
        { Value = phoneNumber }
        );

        cmd.Parameters.Add(new SqlParameter("@BotKey", SqlDbType.NVarChar)
        { Value = botKey }
        );

        SqlParameter outputParam = cmd.Parameters.Add("@ChatId", SqlDbType.NVarChar, 50);
        outputParam.Direction = ParameterDirection.Output;

        await cmd.ExecuteNonQueryAsync();
        if (outputParam.Value == DBNull.Value)
        {
            throw new NotFoundException("User is not subscribed to the provided bot");
        }

        return outputParam.Value.ToString()!;
    }

    /// <summary>
    /// Sending message by adding the message to the Queue Table (ReadyTable)
    /// </summary>
    /// <param name="message">The <see cref="TelegramMessage"/> object containing the message content and recipient details. Cannot be null.</param>
    /// <returns>Returns the ID of the inserted row (reference number), or null if the operation fails</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int?> SendMessage(TelegramMessage message)
    {
        IDbConnection conn = await _connectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_EnqueueOrArchiveIfDuplicate";

        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int)
        { Value = message.CustomerId }
        );
        cmd.Parameters.Add(new SqlParameter("@ChatId", SqlDbType.NVarChar)
        { Value = message.ChatId }
        );
        cmd.Parameters.Add(new SqlParameter("@BotKey", SqlDbType.NVarChar)
        { Value = message.BotKey }
        );
        cmd.Parameters.Add(new SqlParameter("@MessageText", SqlDbType.NVarChar)
        { Value = message.MessageText }
        );
        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.NVarChar)
        { Value = message.PhoneNumber }
        );
        cmd.Parameters.Add(new SqlParameter("@MsgType", SqlDbType.Char)
        { Value = message.MessageType }
        );

        cmd.Parameters.Add(new SqlParameter("@CampaignId", SqlDbType.NVarChar)
        { Value = message.CampaignId }
        );

        cmd.Parameters.Add(new SqlParameter("@CampDescription", SqlDbType.NVarChar)
        { Value = message.CampDescription }
        );

        cmd.Parameters.Add(new SqlParameter("@Priority", SqlDbType.Int)
        { Value = message.Priority }
        );

        cmd.Parameters.Add(new SqlParameter("@IsSystemApproved", SqlDbType.Bit)
        { Value = message.IsSystemApproved }
        );

        return Convert.ToInt32(
            await cmd.ExecuteScalarAsync()
        );
    }

    public Task<IEnumerable<int>> SendMessages(IEnumerable<TelegramMessage> messages)
    {
        throw new NotImplementedException();
    }
}
