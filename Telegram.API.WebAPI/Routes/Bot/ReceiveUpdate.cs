using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.API.Application.CQRS.Commands.Bots;
using Telegram.API.Domain.Entities.Telegram;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Models;

namespace Telegram.API.WebAPI.Routes.Bot;

public class ReceiveUpdate
{
    public static async Task<IResult> RegisterRoute(
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<ReceiveUpdateCommand> validator)
    {

        // 1) Pull from route & header (no parameters needed)
        string? publicId = httpContext.Request.RouteValues.TryGetValue("PublicId", out object? pidObj)
            ? pidObj?.ToString()
            : null;

        httpContext.Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out Microsoft.Extensions.Primitives.StringValues secretVals);
        string? secretToken = secretVals.Count > 0 ? secretVals[0] : null;

        // 2) Endpoint-scoped snake_case deserialization
        JsonSerializerOptions opts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };

        TelegramUpdate? update = await JsonSerializer.DeserializeAsync<TelegramUpdate>(
           httpContext.Request.Body, opts);

        // 3) Assemble command
        ReceiveUpdateCommand request = new(
            PublicId: publicId!,
            SecretToken: secretToken,
            Update: update!
        );

        ValidationResult validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                   .Select(e => e.ErrorMessage)
                   .ToList();

            throw new CustomValidationException("Validation failed ", errors);
        }

        ReceiveUpdateCommandResult result = await mediator.Send(request);

        return Results.Ok(ApiResponse<ReceiveUpdateCommandResult>.SuccessResponse(result));
    }
}
