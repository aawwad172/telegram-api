using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.API.Application.CQRS.Commands.Message;
using Telegram.API.Application.Utilities;
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
        SendCampaignMessageCommand sanitizedRequest = CommandsSanitizer.Sanitize(request);
        ValidationResult validationResult = validator.Validate(sanitizedRequest);
        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            throw new CustomValidationException("Validation failed ", errors);
        }

        SendCampaignMessageCommandResult result = await mediator.Send(sanitizedRequest);
        return Results.Ok(
            ReferenceApiResponse.SuccessResponse(
                referenceNumber: result.CampaignId.ToString()
            )
        );
    }
}
