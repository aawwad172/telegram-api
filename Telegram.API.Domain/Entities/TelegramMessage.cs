using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Entities;

/// <summary>
/// Represents a Telegram message stored in the database.
/// </summary>
public class TelegramMessage : ITelegramMessage

{
    /// <summary>
    /// Unique customer identifier.  
    /// This value is derived from the combination of username and password.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Telegram chat identifier (provided by the user).
    /// </summary>
    public string ChatId { get; set; } = null!;

    /// <summary>
    /// Telegram bot API key (provided by the user).
    /// </summary>
    public required string BotKey { get; set; }

    /// <summary>
    /// Content of the Telegram message (provided by the user).
    /// </summary>
    public required string MessageText { get; set; }

    /// <summary>
    /// Recipient's phone number (provided by the user).
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Indicates the type of message.  
    /// Always set to <c>'A'</c> when coming from the API.
    /// </summary>
    public string MessageType { get; set; } = "A";

    public string CampaignId { get; set; } = string.Empty;
    public string CampDescription { get; set; } = string.Empty;
    public required int Priority { get; set; }
    public required bool IsSystemApproved { get; set; }
}