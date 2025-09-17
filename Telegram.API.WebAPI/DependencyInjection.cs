using A2ASMS.Utility.Logger;
using FluentValidation;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Application.CQRS.Commands.Bots;
using Telegram.API.Application.CQRS.Queries;
using Telegram.API.Application.CQRS.Queries.Bots;
using Telegram.API.Domain.Settings;
using Telegram.API.Infrastructure.Persistence;
using Telegram.API.WebAPI.Validators.Commands.Bot;
using Telegram.API.WebAPI.Validators.Commands.Messages;
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

        A2ALoggerConfig.LogPath = appSettings!.LogPath;
        A2ALoggerConfig.FlushInterval = appSettings.LogFlushInterval;
        A2ALoggerConfig.LogEnabled = true;

        services.AddHealthChecks()
                .AddCheck<DbConnectionHealthCheck>("Database Connection");

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        services.AddTransient<IValidator<SendMessageCommand>, SendMessageCommandValidator>();
        services.AddTransient<IValidator<SubscriptionInfoQuery>, SubscriptionInfoQueryValidator>();
        services.AddTransient<IValidator<SendBatchMessagesCommand>, SendBatchMessagesCommandValidator>();
        services.AddTransient<IValidator<SendCampaignMessageCommand>, SendCampaignMessageCommandValidator>();
        services.AddTransient<IValidator<GetWebhookInfoQuery>, GetWebhookInfoQueryValidator>();
        services.AddTransient<IValidator<RegisterBotCommand>, RegisterBotCommandValidator>();
        services.AddTransient<IValidator<ReceiveUpdateCommand>, ReceiveUpdateCommandValidator>();

        return services;
    }
}
