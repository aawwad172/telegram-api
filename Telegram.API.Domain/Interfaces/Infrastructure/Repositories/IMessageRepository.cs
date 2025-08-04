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

    /// <summary>
    /// Getting the chat id from the phone number and the bot key.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="botKey"></param>
    /// <returns>ChatId of telegram for the number</returns>
    Task<string?> GetChatId(string phoneNumber, string botKey);

    //Task<string> SendCampaign(CampaignMessage messege);
}
