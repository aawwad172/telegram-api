using Microsoft.Data.SqlClient;
using System.Data;
using System.Runtime.CompilerServices;
using Telegram.API.Application.HelperServices;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Infrastructure.Persistence.Repositories;

public class MessageRepository(
    IDbConnectionFactory connectionFactory,
    IJsonFileRepository jsonFileRepository) : IMessageRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IJsonFileRepository _jsonFileRepository = jsonFileRepository;
    private readonly string _folderPath = "C:\\A2A_Telegram_Message\\A2A TelegramAPI - Async\\";

    public async Task SendBatchMessagesAsync<T>(TelegramMessagePackage<T> messages)
    {
        string finalName = FileNameHelper.ComposeCampaignFileName(messages.CampaignId);
        string fullPath = Path.Combine(_folderPath, finalName);

        // 1) Insert DB row, storing the same path (or store just finalName if you prefer)
        await AddBatchFileAsync(messages, fullPath);

        // 2) Save the file
        await _jsonFileRepository.SaveToFileAsync(messages.Items, fullPath);
    }

    /// <summary>
    /// Sending message by adding the message to the Queue Table (ReadyTable)
    /// </summary>
    /// <param name="message">The <see cref="TelegramMessage"/> object containing the message content and recipient details. Cannot be null.</param>
    /// <returns>Returns the ID of the inserted row (reference number), or null if the operation fails</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> SendMessageAsync(TelegramMessage message)
    {
        IDbConnection conn = await _connectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_EnqueueOrArchiveIfDuplicate";

        cmd.Parameters.Add(new SqlParameter("@CustId", SqlDbType.Int)
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

    private async Task AddBatchFileAsync<T>(TelegramMessagePackage<T> messages, string fullPath)
    {
        IDbConnection conn = await _connectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_AddBatchFile";

        cmd.Parameters.Add(new SqlParameter("@CustId", SqlDbType.Int)
        { Value = messages.CustomerId }
        );

        cmd.Parameters.Add(new SqlParameter("@BotKey", SqlDbType.NVarChar)
        { Value = messages.BotKey }
        );

        cmd.Parameters.Add(new SqlParameter("@MsgText", SqlDbType.NVarChar)
        { Value = messages.MessageText }
        );

        cmd.Parameters.Add(new SqlParameter("@MsgType", SqlDbType.NVarChar)
        { Value = messages.MessageType }
        );

        cmd.Parameters.Add(new SqlParameter("@CampaignId", SqlDbType.NVarChar)
        { Value = messages.CampaignId }
        );

        cmd.Parameters.Add(new SqlParameter("@CampDesc", SqlDbType.NVarChar)
        { Value = messages.CampDescription }
        );

        cmd.Parameters.Add(new SqlParameter("@Priority", SqlDbType.SmallInt)
        { Value = messages.Priority }
        );

        cmd.Parameters.Add(new SqlParameter("@IsSystemApproved", SqlDbType.Bit)
        { Value = messages.IsSystemApproved }
        );

        cmd.Parameters.Add(new SqlParameter("@IsAdminApproved", SqlDbType.Bit)
        { Value = messages.IsAdminApproved }
        );

        cmd.Parameters.Add(new SqlParameter("@ScheduledSendDateTime", SqlDbType.DateTime2)
        { Value = messages.ScheduledSendDateTime }
        );
        cmd.Parameters.Add(new SqlParameter("@FilePath", SqlDbType.NVarChar)
        { Value = $"{fullPath}{messages.CampaignId}.json" }
        );

        cmd.Parameters.Add(new SqlParameter("@FileType", SqlDbType.NVarChar)
        { Value = messages.FileType }
        );

        cmd.Parameters.Add(new SqlParameter("@IsProcessed", SqlDbType.Bit)
        { Value = messages.IsProcessed }
        );

        await cmd.ExecuteNonQueryAsync();
    }
}
