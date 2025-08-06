using MediatR;
using Telegram.API.Domain.Interfaces;

namespace Telegram.API.Application.CQRS.Commands;

public sealed record SendMessageCommand : IRequest<SendMessageCommandResult>, IBaseCQRS
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BotKey { get; init; }
    public required string PhoneNumber { get; set; }
    public required string MessageText { get; set; }
}

public sealed record SendMessageCommandResult(string ReferenceNumber);

