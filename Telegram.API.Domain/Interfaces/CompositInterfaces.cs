using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Interfaces;

// Message data addressed by phone
public interface IPhoneMessageData :
    IHasMessageText, IHasPhoneNumber
{ }

// Message data addressed by chat id
public interface IChatMessageData :
    IHasMessageText, IHasEncryptedBotKey, IHasChatId
{ }

// Operational knobs for dispatching
public interface IDispatchInfo :
    IHasPriority, IHasApprovalStatus
{ }

// Optional business tagging
public interface IBusinessTags :
    IHasCamaignId, IHasMessageType
{ }
