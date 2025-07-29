using MediatR;

namespace Telegram.API.Application.CQRS.Commands;

public record SendMessageCommand : IRequest<SendMessageCommandResult>
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string PhoneNumber { get; set; }
    public required string MessageText { get; set; }
    public required string BotKey { get; set; }
}

public record SendMessageCommandResult(string ReferenceNumber);

