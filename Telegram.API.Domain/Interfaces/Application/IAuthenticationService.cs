namespace Telegram.API.Domain.Interfaces.Application;

public interface IAuthenticationService
{
    Task<int> AuthenticateAsync(string username, string password);
}
