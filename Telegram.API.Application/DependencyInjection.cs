using Microsoft.Extensions.DependencyInjection;
using Telegram.API.Application.CQRS.CommandHandlers;
using Telegram.API.Application.CQRS.CommandHandlers.Bots;
using Telegram.API.Application.CQRS.CommandHandlers.Messages;
using Telegram.API.Application.CQRS.QueryHandlers;
using Telegram.API.Application.CQRS.QueryHandlers.Bots;
using Telegram.API.Application.HelperServices;
using Telegram.API.Application.Utilities;
using Telegram.API.Domain.Interfaces.Application;

namespace Telegram.API.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services here
        // Example: services.AddScoped<IMyApplicationService, MyApplicationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Register Command and Query handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(SendMessageCommandHandler).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(SubscriptionInfoQueryHandler).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(SendBatchMessageCommandHandler).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(SendCampaignMessageCommandHandler).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(GetWebhookInfoQueryHandler).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(RegisterBotCommandHandler).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(PortalSendCampaignCommandHandler).Assembly);
        });

        MapsterConfiguration.RegisterMappings();

        return services;
    }
}
