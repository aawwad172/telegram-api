using MediatR;
using Telegram.API.Application.CQRS.Commands;

namespace Telegram.API.Application.CQRS.CommandHandlers
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageCommandResult>
    {
        public Task<SendMessageCommandResult> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
