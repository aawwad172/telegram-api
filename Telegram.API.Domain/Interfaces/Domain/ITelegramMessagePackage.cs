namespace Telegram.API.Domain.Interfaces.Domain;

public interface ITelegramMessagePackage<TItem>
    : IHasCamaignId,
    IHasItems<TItem>,
    IHasCustomerId,
    IHasApprovalStatus,
    IHasMessageType,
    IHasPriority
{
}
