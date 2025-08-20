using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Entities;

public class BatchMessage : IHasMessageText, IHasChatId, IHasPhoneNumber
{
    public required string ChatId { get; init; }
    public required string MessageText { get; init; }

    public required string PhoneNumber { get; init; }
}
