using Telegram.API.Domain.Entities.Fields;

namespace Telegram.API.Domain.Interfaces.Domain;

public interface IHasMessageText
{
    string MessageText { get; }
}
public interface IHasPhoneNumber
{
    string PhoneNumber { get; }
}

public interface IHasBotId
{
    int BotId { get; }
}

public interface IHasChatId
{
    string ChatId { get; }
}
public interface IHasBotKey
{
    string BotKey { get; }
}

public interface IHasEncryptedBotKey
{
    string EncryptedBotKey { get; }
}
public interface IHasCustomerId
{
    int CustomerId { get; }
}

public interface IHasEncryptedCustomerId
{
    string EncryptedCustomerId { get; }
}

public interface IHasPriority
{
    int Priority { get; }
}
public interface IHasApprovalStatus
{
    bool IsSystemApproved { get; }
}
public interface IHasCampaignId
{
    string CampaignId { get; }
}
public interface IHasMessageType
{
    string MessageType { get; }
}

public interface IHasCampDescription
{
    string? CampDescription { get; }
}

public interface IHasSchedule
{
    public DateTime? ScheduledDatetime { get; }
}

public interface IHasItems<TItem>
{
    List<TItem> Items { get; }
}

public interface IHasRemoveDuplicates
{
    bool? RemoveDuplicates { get; }
}

public interface IHasSplitBulk
{
    SplitBulk? SplitBulk { get; }
}
