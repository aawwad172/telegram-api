using FluentValidation;
using MediatR;

namespace Telegram.API.WebAPI.Interfaces;

public interface IRoute<TRequest>
{
    static abstract Task<IResult> RegisterRoute(IMediator mediator, IValidator<TRequest> validator, TRequest request);
}
