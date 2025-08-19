using MediatR;
using Microsoft.Identity.Client;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Application.CQRS.Commands;

public sealed record SendCampaignMessageCommand
    : IRequest<SendCampaignMessageCommandResult>,
    IAuthenticatedBotRequest,
    ICampaignMessage,
    IHasItems<CampaignMessageItem>
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BotKey { get; init; }
    public string? CampDescription { get; init; } = null;
    public DateTime? ScheduledDatetime { get; init; } = null;
    public required string MessageText { get; init; }
    public required List<CampaignMessageItem> Items { get; init; } = [];
}

public sealed record SendCampaignMessageCommandResult(string CampaignId);

public sealed record CampaignMessageItem(string PhoneNumber);
