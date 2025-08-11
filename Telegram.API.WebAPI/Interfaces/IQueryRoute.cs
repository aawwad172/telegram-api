﻿using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Telegram.API.WebAPI.Interfaces;

public interface IQueryRoute<TQuery> where TQuery : notnull
{
    static abstract Task<IResult> RegisterRoute(
    [AsParameters] TQuery query,
    [FromServices] IMediator mediator);
}
