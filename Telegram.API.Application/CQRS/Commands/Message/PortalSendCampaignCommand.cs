using MediatR;
using Telegram.API.Domain.Entities.Fields;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Application.CQRS.Commands.Message;

public sealed record PortalSendCampaignMessageCommand
    : IRequest<PortalSendCampaignCommandResult>,
    IHasItems<CampaignMessageItem>,
    IPortalCampaignMessage
{
    public int BotId { get; init; }
    public required string EncryptedCustomerId { get; init; }
    public required string MessageText { get; init; }
    public required List<CampaignMessageItem> Items { get; init; }
    public string? CampDescription { get; init; }
    public DateTime? ScheduledDatetime { get; init; }
    public SplitBulk? SplitBulk { get; init; }
    public bool? RemoveDuplicates { get; init; }
}

public sealed record PortalSendCampaignCommandResult(string ReferenceNumber);
