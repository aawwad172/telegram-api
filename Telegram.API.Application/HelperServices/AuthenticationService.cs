using A2A.Utils.CryptorRelease2;
using Telegram.API.Domain.Entities;
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
    public async Task<Customer> AuthenticateAsync(string username, string password)
    {
        Customer? customer = await _customerRepository.GetCustomerByUsernameAsync(username) ?? throw new NotFoundException("User not found");

        // Hash the password and return the CustomerId if the password matches
        string encryptedPassword = _encryptionEngine.Encrypt(password);

        if (encryptedPassword != customer.PasswordHash)
            throw new UnauthenticatedException("Invalid username or password");

        if (!customer.IsActive)
            throw new UnauthenticatedException("Customer is not active");

        if (customer.IsBlocked)
            throw new UnauthenticatedException("Customer is blocked");

        if (!customer.IsTelegramActive)
            throw new UnauthenticatedException("Customer is not subscribed in Telegram");

        return customer;
    }

    public async Task<Bot?> ValidateBotKeyAsync(string botKey, int customerId)
    {
        string encryptedPassword = _encryptionEngine.Encrypt(botKey);
        // It will return null if not found
        return await _botRepository.GetBotByKey(encryptedPassword, customerId);
    }
}
