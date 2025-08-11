namespace Telegram.API.Domain.Interfaces.Application;

public interface ICredentials
{
    public string Username { get; init; }
    public string Password { get; init; }
}
