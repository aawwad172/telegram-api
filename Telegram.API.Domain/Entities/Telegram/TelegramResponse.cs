using System.Text.Json.Serialization;

namespace Telegram.API.Domain.Entities.Telegram;

public class TelegramResponse<T>
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("result")]
    public required T Result { get; set; }

    [JsonPropertyName("error_code")]
    public int? ErrorCode { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
