using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.API.Application.CQRS.Queries.Bots;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Interfaces;
using Telegram.API.WebAPI.Models;

namespace Telegram.API.WebAPI.Routes.Bot;

public class GetWebhookInfo : IParameterizedQueryRoute<GetWebhookInfoQuery>
{
    public static async Task<IResult> RegisterRoute(
        [AsParameters] GetWebhookInfoQuery query,
        [FromServices] IMediator mediator,
        [FromServices] FluentValidation.IValidator<GetWebhookInfoQuery> validator)
    {
        FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(query);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

            throw new CustomValidationException("Validation failed {errors}", errors);
        }

        GetWebhookInfoQueryResult result = await mediator.Send(query);

        return Results.Ok(ApiResponse<WebhookInfo?>.SuccessResponse(result.Result.Result));
    }
}
