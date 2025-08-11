using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Interfaces;

// Message data addressed by phone
public interface IPhoneMessageData :
    IMessageText, IBotKey, IPhoneNumber
{ }

// Message data addressed by chat id
public interface IChatMessageData :
    IMessageText, IBotKey, IChatId
{ }

// Operational knobs for dispatching
public interface IDispatchInfo :
    IPriority, IApprovalStatus
{ }

// Optional business tagging
public interface IBusinessTags :
    ICampaignInfo, IMessageType
{ }
