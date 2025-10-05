using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.API.Application.CQRS.Commands.Message;
using Telegram.API.Application.Utilities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Interfaces;
using Telegram.API.WebAPI.Models;

namespace Telegram.API.WebAPI.Routes.Messages;

public class PortalSendCampaignMessage : ICommandRoute<PortalSendCampaignMessageCommand>
{
    public static async Task<IResult> RegisterRoute(
        [FromBody] PortalSendCampaignMessageCommand request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<PortalSendCampaignMessageCommand> validator)
    {
        PortalSendCampaignMessageCommand sanitizedRequest = CommandsSanitizer.Sanitize(request);
        ValidationResult validationResult = validator.Validate(sanitizedRequest);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            throw new CustomValidationException("Validation failed ", errors);
        }

        PortalSendCampaignCommandResult result = await mediator.Send(sanitizedRequest);
        return Results.Ok(
            ReferenceApiResponse.SuccessResponse(
                referenceNumber: result.ReferenceNumber
            )
        );
    }
}
