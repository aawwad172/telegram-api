using MediatR;
using Telegram.API.Domain.Entities.Fields;
using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Application.CQRS.Commands.Message;

public sealed record PortalSendCampaignCommand : IRequest<PortalSendCampaignCommandResult>, IHasItems<CampaignMessageItem>
{
    public required string CustomerId { get; set; }
    public int BotId { get; set; }
    public DateTime? ScheduledDatetime { get; set; }
    public bool? RemoveDuplicates { get; set; }

    public SplitBulk SplitBulk { get; set; } = null;

    public List<CampaignMessageItem> Items => throw new NotImplementedException();
}

public sealed record PortalSendCampaignCommandResult
{

}
