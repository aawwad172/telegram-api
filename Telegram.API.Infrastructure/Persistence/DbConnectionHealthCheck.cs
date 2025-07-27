using Microsoft.Extensions.Diagnostics.HealthChecks;
using Telegram.API.Domain.Interfaces.Infrastructure;

namespace Telegram.API.Infrastructure.Persistence;

public class DbConnectionHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DbConnectionHealthCheck(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            return HealthCheckResult.Healthy("SQL Server connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Unable to connect to SQL Server: " + ex.Message);
        }
    }
}