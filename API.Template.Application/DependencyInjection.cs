using Microsoft.Extensions.DependencyInjection;

namespace API.Template.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services here
        // Example: services.AddScoped<IMyApplicationService, MyApplicationService>();
        
        return services;
    }
}
