namespace Telegram.API.Domain.Interfaces.Domain;

public interface IMessageText
{
    string MessageText { get; }
}
public interface IPhoneNumber
{
    string PhoneNumber { get; }
}

public interface IChatId
{
    string ChatId { get; }
}
public interface IBotKey
{
    string BotKey { get; }
}
public interface ICustomerId
{
    string CustomerId { get; }
}
public interface IPriority
{
    int Priority { get; }
}
public interface IApprovalStatus
{
    bool IsSystemApproved { get; }
}
public interface ICampaignInfo
{
    string CampaignId { get; }
    string CampDescription { get; }
}
public interface IMessageType
{
    string MessageType { get; }
}
