using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Application;

public interface IAuthenticationService
{
    Task<Customer> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<Bot> ValidateBotIdAsync(int BotId, int customerId, CancellationToken cancellationToken = default);
    string Encrypt(string botKey, CancellationToken cancellationToken = default);
    string Decrypt(string encryptedBotKey, CancellationToken cancellationToken = default);
}
