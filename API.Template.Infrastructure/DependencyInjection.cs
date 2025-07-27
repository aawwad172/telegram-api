using API.Template.Domain.Interfaces.Infrastructure;
using API.Template.Domain.Utilities;
using API.Template.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Template.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register your infrastructure services here
        // Example: services.AddSingleton<IMyService, MyService>();

        services.AddTransient<IDbConnectionFactory>(sp =>
                new DbConnectionFactory(
                    Config.ConnectionStrings.ConStr
                )
        );
        services.AddTransient<DbConnectionHealthCheck>();


        return services;
    }
}
