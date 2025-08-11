using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Entities;

public class BatchMessage : IMessageText, IPhoneNumber
{
    public required string PhoneNumber { get; init; }
    public required string MessageText { get; init; }
}
