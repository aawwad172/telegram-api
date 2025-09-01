using MediatR;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Entities.Telegram;
using Telegram.API.Domain.Interfaces.Application;

namespace Telegram.API.Application.CQRS.Queries.Bots;

public sealed record GetWebhookInfoQuery() : IRequest<GetWebhookInfoQueryResult>, IAuthenticatedBotRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BotKey { get; init; }
}

public sealed record GetWebhookInfoQueryResult(TelegramResponse<WebhookInfo?> Result);