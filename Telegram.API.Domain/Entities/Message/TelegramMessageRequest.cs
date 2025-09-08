using System.Text.Json.Serialization;

namespace Telegram.API.Domain.Entities.Message;

public class TelegramMessageRequest
{
    [JsonPropertyName("chat_id")]
    // This is string becuase Telegram API allows both numeric and string chat IDs, but for any future changes, it is more flexible to use string.
    public required string ChatId { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

