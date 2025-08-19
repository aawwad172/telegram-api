using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Entities;

public class CampaignMessage : IHasChatId, IHasPhoneNumber
{
    public required string ChatId { get; init; }

    public required string PhoneNumber { get; init; }
}
