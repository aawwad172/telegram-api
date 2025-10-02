namespace Telegram.API.Domain.Interfaces.Domain;

public interface ITelegramMessagePackage<TItem>
    : IHasCampaignId,
    IHasItems<TItem>,
    IHasCustomerId,
    IHasApprovalStatus,
    IHasMessageType,
    IHasPriority
{
}
