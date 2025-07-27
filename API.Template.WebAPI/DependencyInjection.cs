using API.Template.Domain.Utilities;
using System.Runtime.CompilerServices;

namespace API.Template.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebAPIServices(this IServiceCollection services, IConfiguration configuration)
    {

        Config.ConnectionStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>()!;
        Config.AppConfig = configuration.GetSection("AppConfig").Get<AppConfig>()!;
        Config.TelegramGatewayConfig = configuration.GetSection("TelegramGatewayConfig").Get<TelegramGatewayConfig>()!;

        return services;
    }
}
