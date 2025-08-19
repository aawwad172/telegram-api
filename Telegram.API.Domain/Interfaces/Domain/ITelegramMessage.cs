namespace Telegram.API.Domain.Interfaces.Domain;

public interface ITelegramMessage :
    IHasMessageText,
    IHasBotKey,
    IHasPhoneNumber,
    IHasChatId,
    IHasCustomerId,
    IDispatchInfo,
    IBusinessTags,
    IHasMessageType
{
}
