using FluentValidation;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Domain.Utilities;
using Telegram.API.WebAPI.Validators.Commands;

namespace Telegram.API.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebAPIServices(this IServiceCollection services, IConfiguration configuration)
    {

        Config.ConnectionStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>()!;
        Config.AppConfig = configuration.GetSection("AppConfig").Get<AppConfig>()!;
        Config.TelegramGatewayConfig = configuration.GetSection("TelegramGatewayConfig").Get<TelegramGatewayConfig>()!;

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        services.AddTransient<IValidator<SendMessageCommand>, SendMessageCommandValidator>();

        return services;
    }
}
