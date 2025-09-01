using MediatR;
using Telegram.API.Application.CQRS.Commands.Bots;

namespace Telegram.API.Application.CQRS.CommandHandlers.Bots;

public class RegisterBotCommandHandler : IRequestHandler<RegisterBotCommand, RegisterBotCommandResult>
{
    public Task<RegisterBotCommandResult> Handle(RegisterBotCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
