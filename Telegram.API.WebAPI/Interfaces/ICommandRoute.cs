using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Telegram.API.WebAPI.Interfaces;

public interface ICommandRoute<TRequest> where TRequest : notnull
{
    static abstract Task<IResult> RegisterRoute(
        [FromBody] TRequest request,
        IMediator mediator,
        IValidator<TRequest> validator);
}
