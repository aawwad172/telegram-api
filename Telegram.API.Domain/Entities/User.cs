namespace Telegram.API.Domain.Entities;

public sealed class User
{
    public required int BotId { get; set; }
    public required string ChatId { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime LastSeenDateTime { get; set; }
    public bool IsActive { get; set; }
}
