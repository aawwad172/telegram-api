using System;
using System.Runtime.Serialization;

namespace Telegram.API.Domain.Exceptions;

public class TelegramApiException : Exception
{
    public TelegramApiException(string? message) : base(message)
    {
    }

    public TelegramApiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
