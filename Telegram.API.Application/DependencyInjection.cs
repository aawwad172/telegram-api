using Microsoft.Extensions.DependencyInjection;

namespace Telegram.API.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services here
        // Example: services.AddScoped<IMyApplicationService, MyApplicationService>();
        
        return services;
    }
}
