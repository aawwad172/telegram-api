using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        // Register your infrastructure services
        services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();

        services.AddTransient<DbConnectionHealthCheck>();
        services.AddTransient<IMessageRepository, MessageRepository>();
        services.AddTransient<IUsersRepository, UsersRepository>();
        services.AddTransient<IRecipientRepository, RecipientRepository>();
        services.AddTransient<IJsonFileRepository, JsonFileRepository>();
        services.AddTransient<IBotRepository, BotRepository>();

        services.AddOptions<TelegramOptions>()
                .Bind(configuration.GetRequiredSection(nameof(TelegramOptions)))
                .Validate(o => Uri.TryCreate(o.TelegramApiBaseUrl, UriKind.Absolute, out _), "TelegramApiBaseUrl must be a valid absolute URI.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.BulkFolderPath), "BulkFolderPath is required.")
                .ValidateOnStart();

        services.AddHttpClient<ITelegramClient, TelegramClient>((serviceProvider, client) =>
        {
            TelegramOptions opts = serviceProvider.GetRequiredService<IOptionsMonitor<TelegramOptions>>().CurrentValue;

            client.BaseAddress = new Uri(opts!.TelegramApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
