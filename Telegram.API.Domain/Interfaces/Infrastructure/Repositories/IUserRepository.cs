using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IUserRepository
{
    /// <summary>
    /// Gets the user information from the phone number and the bot id.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="botId"></param>
    /// <returns>User entity containing chat information, or null if not found</returns>
    Task<TelegramUserChat?> GetUserAsync(string phoneNumber, int botId);

    /// <summary>Returns a phone->chatId map (null if not found) in one round-trip.</summary>
    Task<IDictionary<string, string?>> GetChatIdsAsync(IEnumerable<string> phoneNumbers, int botId);
}
