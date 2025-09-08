using MediatR;
using Telegram.API.Domain.Entities.Bot;
using Telegram.API.Domain.Entities.Message;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Application.CQRS.Queries.Bots;

public sealed record GetWebhookInfoQuery() : IRequest<GetWebhookInfoQueryResult>, ICredentials, IHasBotId
{
    public required string Username { get; init; }
    public required string Password { get; init; }

    public required int BotId { get; init; }
}

public sealed record GetWebhookInfoQueryResult(TelegramResponse<WebhookInfo?> Result);