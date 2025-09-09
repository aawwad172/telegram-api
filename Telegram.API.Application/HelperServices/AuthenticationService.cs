using A2A.Utils.CryptorRelease2;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Entities.Bot;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.HelperServices;

public class AuthenticationService(ICustomerRepository customerRepository, IBotRepository botRepository) : IAuthenticationService
{
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly IBotRepository _botRepository = botRepository;
    private readonly EncryptionEngine _encryptionEngine = new();

    /// <summary>
    /// Checks the provided username and password against the authentication system.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns>Returns the CustomerId</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<Customer> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        Customer? customer = await _customerRepository.GetCustomerByUsernameAsync(username, cancellationToken) ?? throw new NotFoundException("User not found");

        // Hash the password and return the CustomerId if the password matches
        string encryptedPassword = _encryptionEngine.Encrypt(password);

        if (encryptedPassword != customer.PasswordHash)
            throw new UnauthenticatedException("Invalid username or password");

        if (!customer.IsActive)
            throw new UnauthenticatedException("Customer is not active");

        if (customer.IsBlocked)
            throw new UnauthenticatedException("Customer is blocked");

        if (!customer.IsTelegramActive)
            throw new UnauthenticatedException("Customer is not subscribed to Telegram feature");

        return customer;
    }

    public string Decrypt(string encryptedBotKey, CancellationToken cancellationToken = default)
        => _encryptionEngine.Decrypt(encryptedBotKey);

    public string Encrypt(string botKey, CancellationToken cancellationToken = default)
        => _encryptionEngine.Encrypt(botKey);

    public async Task<Bot> ValidateBotIdAsync(int botId, int customerId, CancellationToken cancellationToken = default)
    {
        Bot? bot = await _botRepository.GetByIdAsync(botId, customerId, cancellationToken);
        if (bot is null)
            throw new NotFoundException($"There is no bot related with id: {botId} / Bot is not linked with Customer: {customerId}");

        return bot is { IsActive: true } ? bot : throw new BotIsNotActiveException($"Bot is not active for customer {customerId}!");
    }
}
