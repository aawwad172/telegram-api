using MediatR;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Application.CQRS.Commands.Bots;

public sealed record RegisterBotCommand : IRequest<RegisterBotCommandResult>, ICredentials, IHasBotKey
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BotKey { get; init; }
    public required string BotName { get; init; }
}

public sealed record RegisterBotCommandResult(int BotId);