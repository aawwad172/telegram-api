
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


// builder.WebHost.ConfigureKestrel(k =>
// {
//     // e.g., 2 GB
//     k.Limits.MaxRequestBodySize = 2L * 1024 * 1024 * 1024;
// });

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlerMiddleware>();


#region Health Check
// Add health check endpoint
app.MapGet("/health", HealthCheck.RegisterRoute)
    .WithName("HealthCheck")
    .WithTags("healthcheck")
    .WithOpenApi();
#endregion

#region User
app.MapGet("customer/user/subscription", SubscriptionInfo.RegisterRoute)
    .WithName("Subscription Info")
    .WithTags("user")
    .WithOpenApi();
#endregion

#region Messages
app.MapPost("/message/send", SendMessage.RegisterRoute)
    .WithName("Send Message")
    .WithTags("messages")
    .WithOpenApi();

app.MapPost("/message/send/batch", SendBatchMessages.RegisterRoute)
    .WithName("Send Batch Message")
    .WithTags("messages")
    .WithOpenApi();

app.MapPost("/message/send/campaign", SendCampaignMessage.RegisterRoute)
    .WithName("Send Campaign Message")
    .WithTags("messages")
    .WithOpenApi();
#endregion

#region Bots
app.MapPost("/bots/register", RegisterBot.RegisterRoute)
    .WithName("Register Bot")
    .WithTags("bots")
    .WithOpenApi();

app.MapGet("/bots/webhookInfo", GetWebhookInfo.RegisterRoute)
    .WithName("Webhook Info")
    .WithTags("bots")
    .WithOpenApi();
#endregion

app.Run();
