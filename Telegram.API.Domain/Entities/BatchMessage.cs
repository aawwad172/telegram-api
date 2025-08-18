using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Entities;

public class BatchMessage : IMessageText, IChatId
{
    public required string ChatId { get; init; }
    public required string MessageText { get; init; }
}
