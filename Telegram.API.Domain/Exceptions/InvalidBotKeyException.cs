namespace Telegram.API.Domain.Exceptions;

public class InvalidBotKeyException : Exception
{
    public InvalidBotKeyException(string? message) : base(message)
    {
    }

    public InvalidBotKeyException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
