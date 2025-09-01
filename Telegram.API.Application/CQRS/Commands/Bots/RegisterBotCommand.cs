using MediatR;

namespace Telegram.API.Application.CQRS.Commands.Bots;

public sealed record RegisterBotCommand(string BotKey, int CustomerId) : IRequest<RegisterBotCommandResult>;

public sealed record RegisterBotCommandResult(int BotId);