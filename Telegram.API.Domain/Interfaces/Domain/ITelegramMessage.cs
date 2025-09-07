namespace Telegram.API.Domain.Interfaces.Domain;

public interface ITelegramMessage :
    IHasMessageText,
    IHasBotId,
    IHasPhoneNumber,
    IHasChatId,
    IHasCustomerId,
    IDispatchInfo,
    IBusinessTags,
    IHasMessageType
{
}
