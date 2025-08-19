using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Application.HelperServices;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Interfaces;
using Telegram.API.WebAPI.Models;

namespace Telegram.API.WebAPI.Routes.Messages;

public class SendCampaignMessage : ICommandRoute<SendCampaignMessageCommand>
{
    public static async Task<IResult> RegisterRoute(
        [FromBody] SendCampaignMessageCommand request,
        [FromServices] IMediator mediator,
        [FromServices] FluentValidation.IValidator<SendCampaignMessageCommand> validator)
    {
        var sanitizedRequest = CommandsSanitizer.Sanitize(request);
        ValidationResult validationResult = validator.Validate(sanitizedRequest);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            throw new CustomValidationException("Validation failed ", errors);
        }

        return Results.Ok(
            ApiResponse<SendCampaignMessageCommandResult>.SuccessResponse(
                await mediator.Send(sanitizedRequest)
            )
        );
    }
}
