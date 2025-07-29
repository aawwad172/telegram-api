using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;
using A2A.Utils.CryptorRelease2;
using A2A.Utils;

namespace Telegram.API.Application.HelperServices;

public class AuthenticationService(IUserRepository userRepository) : IAuthenticationService
{
    private readonly IUserRepository _userRepository = userRepository;
    /// <summary>
    /// Checks the provided username and password against the authentication system.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns>Returns the CustomerId</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> AuthenticateAsync(string username, string password)
    {
        User user = await _userRepository.GetByUsernameAsync(username);
        if (user is null)
        {
            throw new NotFoundException("User not found");
        }

        // Hash the pawwrod and return the CustomerId if the password matches
        var hashedPassword = CryptionEngine.EncryptData(password, false);

        return hashedPassword == password
            ? throw new UnauthenticatedException("Invalid username or password") :
            user.CustomerId;
    }
}
