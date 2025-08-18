using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IMessageRepository
{
    /// <summary>
    /// Sending message by adding the message to the Queeu Table (ReadyTable)
    /// </summary>
    /// <param name="message"></param>
    /// <returns>It returns the Id of the row that been inserted (reference number)</returns>
    Task<int> SendMessageAsync(TelegramMessage message);

    Task SendBatchMessagesAsync<T>(TelegramMessagePackage<T> messages);
}
