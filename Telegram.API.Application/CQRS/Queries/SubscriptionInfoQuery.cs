using MediatR;
using Telegram.API.Domain.Interfaces;

namespace Telegram.API.Application.CQRS.Queries;

public record SubscriptionInfoQuery : IRequest<SubscriptionInfoQueryResult>, IBaseCQRS
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BotKey { get; init; }
    public required string PhoneNumber { get; init; }
}

public sealed record SubscriptionInfoQueryResult(bool Subscribed, string? ChatId, DateTime? CreationDate);
