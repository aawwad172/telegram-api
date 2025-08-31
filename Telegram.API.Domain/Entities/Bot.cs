namespace Telegram.API.Domain.Entities;

public sealed class Bot
{
    public required int BotId { get; init; }
    public required int CustomerId { get; init; }
    public required string EncryptedBotKey { get; init; }
    public string? WebhookSecret { get; init; }
    public string? WebhookUrl { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreationDateTime { get; init; }
}
