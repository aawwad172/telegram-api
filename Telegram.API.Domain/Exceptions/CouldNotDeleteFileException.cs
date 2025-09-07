namespace Telegram.API.Domain.Exceptions;

public class CouldNotDeleteFileException : Exception
{
    public CouldNotDeleteFileException(string? message) : base(message)
    {
    }

    public CouldNotDeleteFileException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
