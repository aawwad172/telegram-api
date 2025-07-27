using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Template.WebAPI.Routes.HealthCheck;

public class HealthCheck
{
    public static async Task<IResult> RegisterRoute(HealthCheckService healthCheckService)
    {
        var report = await healthCheckService.CheckHealthAsync();
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
