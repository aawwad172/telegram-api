using System;

namespace Telegram.API.Domain.Entities;

public class Bot
{
    public required int BotId { get; set; }
    public required int CustomerId { get; set; }
    public required string EncryptedBotKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? WebhookUrl { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreationDateTime { get; set; }
}
