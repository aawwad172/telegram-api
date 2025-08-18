using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Entities;

public class BatchMessage : IMessageText, IChatId
{
    public required string ChatId { get; set; }
    public required string MessageText { get; set; }
}
