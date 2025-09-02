namespace Telegram.API.Domain.Entities.Telegram;

public class TelegramResponse<T>
{
    public bool Ok { get; set; }

    public required T? Result { get; set; }

    public int? ErrorCode { get; set; }

    public string? Description { get; set; }
}
