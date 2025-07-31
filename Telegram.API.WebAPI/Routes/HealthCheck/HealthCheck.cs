using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Telegram.API.WebAPI.Interfaces;

namespace Telegram.API.WebAPI.Routes.HealthCheck;

public class HealthCheck
{
    public static async Task<IResult> RegisterRoute(
        [FromServices] HealthCheckService service)
    {
        HealthReport report = await service.CheckHealthAsync();
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
