namespace Telegram.API.Domain.Enums;

public enum MessagePriorityEnum
{
    SingleMessage = 6, // Represents the priority for a single message
    BatchMessage = 2, // Represents the priority for a batch of messages
    CampaignMessage = 2, // Represents the priority for a campaign message
    PortalCampaignMessage = 1,
    PortalBatchMessage = 1
}
