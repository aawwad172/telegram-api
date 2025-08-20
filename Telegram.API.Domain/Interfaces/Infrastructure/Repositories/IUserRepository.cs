using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets the user information from the phone number and the bot key.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="botKey"></param>
    /// <returns>User entity containing chat information, or null if not found</returns>
    Task<User?> GetUserAsync(string phoneNumber, string botKey);

    /// <summary>Returns a phone->chatId map (null if not found) in one round-trip.</summary>
    Task<IDictionary<string, string?>> GetChatIdsAsync(IEnumerable<string> phoneNumbers, string botKey);
}
