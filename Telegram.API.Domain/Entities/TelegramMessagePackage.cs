using Telegram.API.Domain.Enums;
using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Entities;

public class TelegramMessagePackage<TItem> : ITelegramMessagePackage<TItem>
{
    public int CustomerId { get; set; }
    public required string EncryptedBotKey { get; set; }
    public string MessageText { get; set; } = string.Empty; // Optional, used for batch messages
    public required string MessageType { get; set; }
    public required string CampaignId { get; set; }
    public string CampDescription { get; set; } = null!;
    public required int Priority { get; set; }
    public required bool IsSystemApproved { get; set; }
    public DateTime? ScheduledSendDateTime { get; set; } = null;
    public List<TItem> Items { get; set; } = [];
    public bool IsAdminApproved => IsSystemApproved;
    public bool IsProcessed { get; set; } = false;
    public string FileType { get; } = typeof(TItem) == typeof(BatchMessage) ? FileTypeEnum.Batch.ToString() : FileTypeEnum.Campaign.ToString();
}
