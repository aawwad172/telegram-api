using MediatR;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Interfaces.Application;

namespace Telegram.API.Application.CQRS.Commands;

public sealed record SendBatchMessageCommand : IRequest<SendBatchMessageCommandResult>, IAuthenticatedBotRequest, IBatch<BatchMessage>
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BotKey { get; init; }
    public required IEnumerable<BatchMessage> Items { get; set; } = [];
}

public sealed record SendBatchMessageCommandResult();
