using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IMessageRepository
{
    /// <summary>
    /// Sending message by adding the message to the Queeu Table (ReadyTable)
    /// </summary>
    /// <param name="message"></param>
    /// <returns>It returns the Id of the row that been inserted (reference number)</returns>
    Task<int> SendMessage(TelegramMessage message);

    /// <summary>
    /// Sending messages by adding the messages to the Queue Table (ReadyTable)
    /// </summary>
    /// <param name="messages"></param>
    /// <returns>It returns array of id's for each message</returns>
    Task<IEnumerable<int>> SendMessages(IEnumerable<TelegramMessage> messages);

    //Task<string> SendCampaign(CampaignMessage messege);
}
