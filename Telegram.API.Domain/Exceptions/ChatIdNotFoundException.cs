namespace Telegram.API.Domain.Exceptions;

public class ChatIdNotFoundException : Exception
{
    public ChatIdNotFoundException(string message) : base(message)
    {
    }
    public ChatIdNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
