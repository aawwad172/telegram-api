using API.Template.Application;
using API.Template.Domain;
using API.Template.Infrastructure;
using API.Template.Infrastructure.Persistence;
using API.Template.WebAPI;
using API.Template.WebAPI.Routes.HealthCheck;

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

// Add health check endpoint
app.MapGet("/health", HealthCheck.RegisterRoute)
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
