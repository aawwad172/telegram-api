using MediatR;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Application.CQRS.Queries;

public sealed record SubscriptionInfoQuery : IRequest<SubscriptionInfoQueryResult>, IAuthenticatedBotRequest, IHasPhoneNumber
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BotKey { get; init; }
    public required string PhoneNumber { get; init; }
}

public sealed record SubscriptionInfoQueryResult(bool Subscribed, string? ChatId, DateTime? CreationDate);
