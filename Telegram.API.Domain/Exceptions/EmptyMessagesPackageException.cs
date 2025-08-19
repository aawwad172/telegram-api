namespace Telegram.API.Domain.Exceptions;

public class EmptyMessagesPackageException : Exception
{
    public EmptyMessagesPackageException(string? message) : base(message)
    {
    }

    public EmptyMessagesPackageException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
