using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;
using Telegram.API.Infrastructure.Persistence;
using Telegram.API.Infrastructure.Persistence.Repositories;

namespace Telegram.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register your infrastructure services here
        // Example: services.AddSingleton<IMyService, MyService>();

        // Register your infrastructure services
        services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();

        services.AddTransient<DbConnectionHealthCheck>();
        services.AddTransient<IMessageRepository, MessageRepository>();
        services.AddTransient<ICustomerRepository, CustomerRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IJsonFileRepository, JsonFileRepository>();
        services.AddTransient<IBotRepository, BotRepository>();

        return services;
    }
}
