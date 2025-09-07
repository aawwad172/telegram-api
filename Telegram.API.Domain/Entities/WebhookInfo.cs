namespace Telegram.API.Domain.Entities;

/// <summary>
/// Shape of Telegram's getWebhookInfo "result".
/// </summary>
public sealed class WebhookInfo
{
    public string Url { get; init; } = "";
    public bool HasCustomCertificate { get; init; }
    public int PendingUpdateCount { get; init; }
    public string? IpAddress { get; init; }
    public int? LastErrorDate { get; init; }                // Unix time (seconds)
    public string? LastErrorMessage { get; init; }
    public int? LastSynchronizationErrorDate { get; init; } // Unix time (seconds)
    public int? MaxConnections { get; init; }
    public string[]? AllowedUpdates { get; init; }
}
