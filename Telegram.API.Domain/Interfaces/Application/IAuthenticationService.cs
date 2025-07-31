using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Application;

public interface IAuthenticationService
{
    Task<User> AuthenticateAsync(string username, string password);
}
