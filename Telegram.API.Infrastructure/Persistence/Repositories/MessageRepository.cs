using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using Telegram.API.Application.HelperServices;
using Telegram.API.Domain.Entities.Message;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;
using Telegram.API.Domain.Settings;

namespace Telegram.API.Infrastructure.Persistence.Repositories;

public class MessageRepository(
    IDbConnectionFactory connectionFactory,
    IJsonFileRepository jsonFileRepository, IOptionsMonitor<TelegramOptions> options) : IMessageRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IJsonFileRepository _jsonFileRepository = jsonFileRepository;
    private readonly IOptionsMonitor<TelegramOptions> _telegramOptions = options;

    public async Task SendBatchMessagesAsync<T>(TelegramMessagePackage<T> messages)
    {
        string finalName = FileNameHelper.ComposeCampaignFileName(messages.CampaignId);
        string fullPath = Path.Combine(_telegramOptions.CurrentValue.BulkFolderPath, finalName);

        // 1) Save the file
        await _jsonFileRepository.SaveToFileAsync(messages.Items, fullPath);

        // 2) Insert DB row, storing the same path (or store just finalName if you prefer)
        try
        {
            await AddBatchFileAsync(messages, fullPath);
        }
        catch (Exception)
        {
            try
            {
                File.Delete(fullPath);
            }
            catch
            {
                throw new CouldNotDeleteFileException(
                    $"Failed to delete the file {fullPath} after an error occurred while inserting into the database."
                );
            }
            throw;
        }
    }

    /// <summary>
    /// Sending message by adding the message to the Queue Table (ReadyTable)
    /// </summary>
    /// <param name="message">The <see cref="TelegramMessage"/> object containing the message content and recipient details. Cannot be null.</param>
    /// <returns>Returns the ID of the inserted row (reference number), or null if the operation fails</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> SendMessageAsync(TelegramMessage message)
    {
        using IDbConnection conn = await _connectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_EnqueueOrArchiveIfDuplicate";

        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int)
        { Value = message.CustomerId }
        );

        cmd.Parameters.Add(new SqlParameter("@ChatId", SqlDbType.NVarChar, 50)
        { Value = message.ChatId }
        );

        cmd.Parameters.Add(new SqlParameter("@BotId", SqlDbType.Int)
        { Value = message.BotId }
        );

        cmd.Parameters.Add(new SqlParameter("@MessageText", SqlDbType.NVarChar, -1)
        { Value = message.MessageText }
        );

        cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.NVarChar, 32)
        { Value = message.PhoneNumber }
        );

        cmd.Parameters.Add(new SqlParameter("@MsgType", SqlDbType.NVarChar, 10)
        { Value = message.MessageType }
        );

        cmd.Parameters.Add(new SqlParameter("@CampaignId", SqlDbType.NVarChar, 128)
        { Value = message.CampaignId }
        );

        cmd.Parameters.Add(new SqlParameter("@CampDescription", SqlDbType.NVarChar, 512)
        { Value = message.CampDescription }
        );

        cmd.Parameters.Add(new SqlParameter("@Priority", SqlDbType.SmallInt)
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
        using IDbConnection conn = await _connectionFactory.CreateOpenConnection();
        using SqlCommand cmd = (SqlCommand)conn.CreateCommand();

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "usp_AddBatchFile";

        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int)
        { Value = messages.CustomerId }
        );

        cmd.Parameters.Add(new SqlParameter("@BotId", SqlDbType.Int)
        { Value = messages.BotId }
        );

        cmd.Parameters.Add(new SqlParameter("@MsgText", SqlDbType.NVarChar, -1)
        { Value = messages.MessageText }
        );

        cmd.Parameters.Add(new SqlParameter("@MsgType", SqlDbType.NVarChar, 10)
        { Value = messages.MessageType }
        );

        cmd.Parameters.Add(new SqlParameter("@CampaignId", SqlDbType.NVarChar, 128)
        { Value = messages.CampaignId }
        );

        cmd.Parameters.Add(new SqlParameter("@CampDescription", SqlDbType.NVarChar, 512)
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
        { Value = (object)messages.ScheduledSendDateTime! ?? DBNull.Value }
        );
        cmd.Parameters.Add(new SqlParameter("@FilePath", SqlDbType.NVarChar)
        { Value = fullPath }
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
