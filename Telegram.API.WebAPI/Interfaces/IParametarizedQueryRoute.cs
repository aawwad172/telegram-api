using FluentValidation;
using MediatR;

namespace Telegram.API.WebAPI.Interfaces;

/// <summary>
/// This interface is used to define a route for a parameterized query so we can validate the query params.
/// </summary>
/// <typeparam name="TQuery"></typeparam>
public interface IParametarizedQueryRoute<TQuery> where TQuery : notnull
{
    static abstract Task<IResult> RegisterRoute(
    [AsParameters] TQuery query,
    IMediator mediator,
    IValidator<TQuery> validator);
}
