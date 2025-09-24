using Telegram.API.Domain.Entities.User;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IRecipientRepository
{
    /// <summary>
    /// Gets the Recipient information from the phone number and the bot id.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="botId"></param>
    /// <returns>Recipient entity containing chat information, or null if not found</returns>
    Task<Recipient?> GetRecipientAsync(string phoneNumber, int botId);

    /// <summary>Returns a phone->chatId map (null if not found) in one round-trip.</summary>
    Task<IDictionary<string, string?>> GetChatIdsAsync(IEnumerable<string> phoneNumbers, int botId);

    /// <summary>
    /// Mark a chat as inactive (e.g., user blocked the bot).
    /// </summary>
    Task DeactivateAsync(
        int botId,
        string chatId,
        CancellationToken ct);

    /// <summary>
    /// Get all active chats for a bot (for broadcast/campaigns).
    /// </summary>
    Task<IReadOnlyCollection<Recipient>> GetActiveChatsAsync(
        int botId,
        CancellationToken ct);
}
