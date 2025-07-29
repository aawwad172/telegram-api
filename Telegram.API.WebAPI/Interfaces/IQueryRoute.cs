using MediatR;

namespace Telegram.API.WebAPI.Interfaces;

public interface IQueryRoute<TQuery> where TQuery : notnull
{
    static abstract Task<IResult> RegisterRoute(
    [AsParameters] TQuery query,
    IMediator mediator);
}
