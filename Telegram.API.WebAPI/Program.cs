using Telegram.API.Application;
using Telegram.API.Domain;
using Telegram.API.Infrastructure;
using Telegram.API.WebAPI;
using Telegram.API.WebAPI.Middlewares;
using Telegram.API.WebAPI.Routes.Bot;
using Telegram.API.WebAPI.Routes.HealthCheck;
using Telegram.API.WebAPI.Routes.Messages;
using Telegram.API.WebAPI.Routes.User;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDomainServices()
                .AddApplicationServices()
                .AddInfrastructureServices(builder.Configuration)
                .AddWebAPIServices(builder.Configuration);


builder.WebHost.ConfigureKestrel(k =>
{
    // e.g., 2 GB
    k.Limits.MaxRequestBodySize = 2L * 1024 * 1024 * 1024;
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlerMiddleware>();

// Create a route group with prefix "/telegram"
RouteGroupBuilder api = app.MapGroup("/api");

#region Health Check
// Add health check endpoint
app.MapGet("/health", HealthCheck.RegisterRoute)
    .WithName("Health Check")
    .WithTags("healthcheck")
    .WithOpenApi();
#endregion

#region User
api.MapGet("customer/recipient/subscription", SubscriptionInfo.RegisterRoute)
    .WithName("Subscription Info")
    .WithTags("user")
    .WithOpenApi();

#endregion

#region Message
api.MapPost("/message/send", SendMessage.RegisterRoute)
    .WithName("Send Message")
    .WithTags("messages")
    .WithOpenApi();

api.MapPost("/message/send/batch", SendBatchMessages.RegisterRoute)
    .WithName("Send Batch Message")
    .WithTags("messages")
    .WithOpenApi();

api.MapPost("/message/send/campaign", SendCampaignMessage.RegisterRoute)
    .WithName("Send Campaign Message")
    .WithTags("messages")
    .WithOpenApi();
#endregion

#region Bot
api.MapPost("/bot/register", RegisterBot.RegisterRoute)
    .WithName("Register Bot")
    .WithTags("bots")
    .WithOpenApi();

api.MapGet("/bot/webhookInfo", GetWebhookInfo.RegisterRoute)
    .WithName("Webhook Info")
    .WithTags("bots")
    .WithOpenApi();
#endregion

#region Portal

#endregion

app.Run();
