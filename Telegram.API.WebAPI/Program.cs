using Telegram.API.Application;
using Telegram.API.Domain;
using Telegram.API.Infrastructure;
using Telegram.API.Infrastructure.Persistence;
using Telegram.API.WebAPI;
using Telegram.API.WebAPI.Routes.HealthCheck;
using Telegram.API.WebAPI.Routes.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDomainServices()
                .AddApplicationServices()
                .AddInfrastructureServices(builder.Configuration)
                .AddWebAPIServices(builder.Configuration);

builder.Services.AddHealthChecks()
                .AddCheck<DbConnectionHealthCheck>("Database Connection");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

# region Health Check
// Add health check endpoint
app.MapGet("/health", HealthCheck.RegisterRoute)
    .WithName("HealthCheck")
    .WithOpenApi();
#endregion

# region Messages
app.MapPost("/message", SendMessage.RegisterRoute)
    .WithName("SendMessage")
    .WithOpenApi();
#endregion

app.Run();
