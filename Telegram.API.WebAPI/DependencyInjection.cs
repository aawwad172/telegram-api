using A2ASerilog;
using FluentValidation;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Application.CQRS.Queries;
using Telegram.API.Domain.Utilities;
using Telegram.API.Infrastructure.Persistence;
using Telegram.API.WebAPI.Validators.Commands;
using Telegram.API.WebAPI.Validators.Queries;

namespace Telegram.API.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebAPIServices(this IServiceCollection services, IConfiguration configuration)
    {

        Config.ConnectionStrings = configuration.GetRequiredSection("ConnectionStrings").Get<ConnectionStrings>()!;
        Config.AppConfig = configuration.GetRequiredSection("AppConfig").Get<AppConfig>()!;
        //Config.TelegramGatewayConfig = configuration.GetRequiredSection("TelegramGatewayConfig").Get<TelegramGatewayConfig>()!;

        LoggerService._logPath = Config.AppConfig.LogPath;
        LoggerService._flushPeriod = Config.AppConfig.LogFlushInterval;

        services.AddHealthChecks()
                .AddCheck<DbConnectionHealthCheck>("Database Connection");

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        services.AddTransient<IValidator<SendMessageCommand>, SendMessageCommandValidator>();
        services.AddTransient<IValidator<SubscriptionInfoQuery>, SubscriptionInfoQueryValidator>();
        return services;
    }
}
