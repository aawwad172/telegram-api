using MediatR;
using Telegram.API.Domain.Interfaces;
using Telegram.API.Domain.Interfaces.Application;

namespace Telegram.API.Application.CQRS.Commands;

public sealed record SendMessageCommand : IRequest<SendMessageCommandResult>, IAuthenticatedBotRequest, IPhoneMessageData
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BotKey { get; init; }
    public required string PhoneNumber { get; init; }
    public required string MessageText { get; init; }
}

public sealed record SendMessageCommandResult(string ReferenceNumber);

