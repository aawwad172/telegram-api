using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.API.Domain.Interfaces.Infrastructure;
using Telegram.API.Domain.Interfaces.Infrastructure.Clients;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;
using Telegram.API.Domain.Settings;
using Telegram.API.Infrastructure.Clients;
using Telegram.API.Infrastructure.Persistence;
using Telegram.API.Infrastructure.Persistence.Repositories;

namespace Telegram.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register your infrastructure services here
        // Example: services.AddSingleton<IMyService, MyService>();

        TelegramOptions? telegramOptions = configuration.GetRequiredSection(nameof(TelegramOptions)).Get<TelegramOptions>();

        // Register your infrastructure services
        services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();

        services.AddTransient<DbConnectionHealthCheck>();
        services.AddTransient<IMessageRepository, MessageRepository>();
        services.AddTransient<ICustomerRepository, CustomerRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IJsonFileRepository, JsonFileRepository>();
        services.AddTransient<IBotRepository, BotRepository>();


        services.AddHttpClient<ITelegramClient, TelegramClient>(c =>
        {
            c.BaseAddress = new Uri(telegramOptions!.TelegramApiBaseUrl);
            c.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
