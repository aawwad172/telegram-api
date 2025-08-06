using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Application;

public interface IAuthenticationService
{
    Task<Customer> AuthenticateAsync(string username, string password);
}
