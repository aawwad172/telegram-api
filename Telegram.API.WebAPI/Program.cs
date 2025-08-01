
using Telegram.API.Application;
using Telegram.API.Domain;
using Telegram.API.Infrastructure;
using Telegram.API.WebAPI;
using Telegram.API.WebAPI.Middlewares;
using Telegram.API.WebAPI.Routes.HealthCheck;
using Telegram.API.WebAPI.Routes.Messages;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDomainServices()
                .AddApplicationServices()
                .AddInfrastructureServices(builder.Configuration)
                .AddWebAPIServices(builder.Configuration);


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

# region Messages
app.MapPost("/message/send", SendMessage.RegisterRoute)
    .WithName("SendMessage")
    .WithTags("messages")
    .WithOpenApi();
#endregion

app.Run();
