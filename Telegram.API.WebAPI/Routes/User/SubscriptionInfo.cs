using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.API.Application.CQRS.Queries;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Interfaces;
using Telegram.API.WebAPI.Models;

namespace Telegram.API.WebAPI.Routes.User;

public class SubscriptionInfo : IParameterizedQueryRoute<SubscriptionInfoQuery>
{
    public static async Task<IResult> RegisterRoute(
        [AsParameters] SubscriptionInfoQuery query,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<SubscriptionInfoQuery> validator)
    {
        ValidationResult validationResult = await validator.ValidateAsync(query);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            throw new CustomValidationException("Validation failed: ", errors);
        }

        return Results.Ok(ApiResponse<SubscriptionInfoQueryResult>.SuccessResponse(await mediator.Send(query)));
    }
}
