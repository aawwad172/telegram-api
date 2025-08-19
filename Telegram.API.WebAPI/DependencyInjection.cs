using A2ASerilog;
using FluentValidation;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Application.CQRS.Queries;
using Telegram.API.Domain.Settings;
using Telegram.API.Infrastructure.Persistence;
using Telegram.API.WebAPI.Validators.Commands;
using Telegram.API.WebAPI.Validators.Queries;

namespace Telegram.API.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebAPIServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<DbSettings>(configuration.GetSection(nameof(DbSettings)));
        services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
        services.Configure<TelegramOptions>(configuration.GetSection(nameof(TelegramOptions)));

        AppSettings? appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();

        LoggerService._logPath = appSettings!.LogPath;
        LoggerService._flushPeriod = appSettings.LogFlushInterval;

        services.AddHealthChecks()
                .AddCheck<DbConnectionHealthCheck>("Database Connection");

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        services.AddTransient<IValidator<SendMessageCommand>, SendMessageCommandValidator>();
        services.AddTransient<IValidator<SubscriptionInfoQuery>, SubscriptionInfoQueryValidator>();
        services.AddTransient<IValidator<SendBatchMessagesCommand>, SendBatchMessagesCommandValidator>();
        services.AddTransient<IValidator<SendCampaignMessageCommand>, SendCampaignMessageCommandValidator>();

        return services;
    }
}
