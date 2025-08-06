namespace Telegram.API.Domain.Entities;

public class Customer
{
    public required int CustomerId { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required bool RequireSystemApprove { get; set; } // If False then you should insert true for the IsSystemApproved field in the TableReady
    public required bool RequireAdminApprove { get; set; }
    public required bool IsActive { get; set; }
    public required bool IsBlocked { get; set; }
    public required bool IsTelegramActive { get; set; }
}
