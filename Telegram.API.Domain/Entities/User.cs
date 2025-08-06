namespace Telegram.API.Domain.Entities;

public class User
{
    public required string ChatId { get; set; }
    public required string PhoneNumber { get; set; }
    public required string BotKey { get; set; }
    public required DateTime CreationDate { get; set; }
}
