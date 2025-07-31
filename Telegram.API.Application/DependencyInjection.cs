using Microsoft.Extensions.DependencyInjection;
using Telegram.API.Application.CQRS.CommandHandlers;
using Telegram.API.Application.HelperServices;
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
        });


        return services;
    }
}
