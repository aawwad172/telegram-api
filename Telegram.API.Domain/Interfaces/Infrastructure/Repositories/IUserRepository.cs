using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Getting the chat id from the phone number and the bot key.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="botKey"></param>
    /// <returns>ChatId of telegram for the number</returns>
    Task<User?> GetUserAsync(string phoneNumber, string botKey);
}
