namespace Telegram.API.Domain.Exceptions;

public class BotIsNotActiveException : Exception
{
    public BotIsNotActiveException(string? message) : base(message)
    {
    }

    public BotIsNotActiveException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
