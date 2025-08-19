using System.Runtime.Serialization;

namespace Telegram.API.Domain.Exceptions;

public class CouldntDeleteFileException : Exception
{
    public CouldntDeleteFileException(string? message) : base(message)
    {
    }

    public CouldntDeleteFileException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
