namespace Telegram.API.Domain.Entities.User;

public sealed class UserTelegramProfile
{
    public int Id { get; init; }
    public bool IsActive { get; init; }
    public bool CanViewContent { get; init; }
    public bool IsPrepaid { get; init; }
    public bool CanViewReports { get; init; }
    public bool CanCreateReports { get; init; }
    public bool HasOtp { get; init; }
    public bool CanManageUsers { get; init; }
    public bool CanManageAdmins { get; init; }
    public bool HasOutbox { get; init; }
    public bool HasSurvey { get; init; }
    public bool CanSendMessage { get; init; }
    public bool CanSendBatch { get; init; }
    public bool CanSendCampaign { get; init; }
    public byte[] RowVer { get; init; } = [];
    public DateTime? UpdatedAt { get; init; }
}
