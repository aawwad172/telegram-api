using MediatR;
using Telegram.API.Application.CQRS.Commands;

namespace Telegram.API.Application.CQRS.CommandHandlers;

public class SendBatchMessageCommandHandler : IRequestHandler<SendBatchMessageCommand, SendBatchMessageCommandResult>
{

    public Task<SendBatchMessageCommandResult> Handle(SendBatchMessageCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
