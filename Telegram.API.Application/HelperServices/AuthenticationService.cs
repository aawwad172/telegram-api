using A2A.Utils.CryptorRelease2;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.HelperServices;

public class AuthenticationService(IUserRepository userRepository) : IAuthenticationService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly EncryptionEngine _encryptionEngine = new();
    /// <summary>
    /// Checks the provided username and password against the authentication system.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns>Returns the CustomerId</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<User> AuthenticateAsync(string username, string password)
    {
        User? user = await _userRepository.GetByUsernameAsync(username) ?? throw new NotFoundException("User not found");

        // Hash the pawwrod and return the CustomerId if the password matches
        string encryptedPassword = _encryptionEngine.Encrypt(password);

        if (encryptedPassword != user.PasswordHash)
            throw new UnauthenticatedException("Invalid username or password");

        if (!user.IsActive)
            throw new UnauthenticatedException("User is not active");

        if (user.IsBlocked)
            throw new UnauthenticatedException("User is blocked");

        if (!user.IsTelegramActive)
            throw new UnauthenticatedException("User is not subscriped in Telegram");

        return user;
    }
}
