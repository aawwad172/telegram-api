using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Utilities;
using Telegram.API.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.API.Infrastructure.Persistence.Repositories;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Infrastructure;

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
        services.AddTransient<IMessageRepository, MessageRepository>();
        services.AddTransient<IUserRepository, UserRepository>();

        return services;
    }
}
