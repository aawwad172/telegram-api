using FluentValidation;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Telegram.API.WebAPI.Interfaces;

namespace Telegram.API.WebAPI.Routes.HealthCheck;

public class HealthCheck : IRoute<HealthCheckService>
{
    public static async Task<IResult> RegisterRoute(IMediator mediator, IValidator<HealthCheckService> validator, HealthCheckService request)
    {
        // This doesn't require a mediator or validator, but they are included to match the interface.
        var report = await request.CheckHealthAsync();
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };

        return Results.Ok(response);
    }
}
