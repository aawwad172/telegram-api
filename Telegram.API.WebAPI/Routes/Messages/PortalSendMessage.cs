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

public class PortalSendMessage : ICommandRoute<PortalSendCampaignCommand>
{
    public static async Task<IResult> RegisterRoute(
        [FromBody] PortalSendCampaignCommand request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<PortalSendCampaignCommand> validator)
    {
        PortalSendCampaignCommand sanitizedRequest = CommandsSanitizer.Sanitize(request);
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
                referenceNumber: result.ReferenceId
            )
        );
    }
}
