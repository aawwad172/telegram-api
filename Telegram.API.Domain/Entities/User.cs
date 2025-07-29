namespace Telegram.API.Domain.Entities;

public class User
{
    /// <summary>
    /// Represent the CustomerId of the User.
    /// </summary>
    public required int CustomerId { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
}
