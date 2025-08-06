namespace Telegram.API.Domain.Interfaces;

public interface IBaseCQRS
{
    public string Username { get; init; }
    public string Password { get; init; }
    public string BotKey { get; init; }
}
