using MediatR;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Application.CQRS.Commands;

public sealed record SendBatchMessagesCommand
    : IRequest<SendBatchMessageCommandResult>,
    IAuthenticatedBotRequest,
    IHasItems<BatchMessageItem>,
    IBatchMessages
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BotKey { get; init; }
    public string? CampDescription { get; init; } = null;
    public DateTime? ScheduledDatetime { get; init; } = null;
    public required List<BatchMessageItem> Items { get; set; } = [];
}

public sealed record SendBatchMessageCommandResult(string CampaignId);

public sealed record BatchMessageItem(string PhoneNumber, string MessageText);
