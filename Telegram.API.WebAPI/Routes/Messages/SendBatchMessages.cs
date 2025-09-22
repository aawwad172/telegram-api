using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Application.HelperServices;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Interfaces;
using Telegram.API.WebAPI.Models;

namespace Telegram.API.WebAPI.Routes.Messages;

public class SendBatchMessages : ICommandRoute<SendBatchMessagesCommand>
{
    public static async Task<IResult> RegisterRoute(
        [FromBody] SendBatchMessagesCommand request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<SendBatchMessagesCommand> validator)
    {
        SendBatchMessagesCommand sanitizedRequest = CommandsSanitizer.Sanitize(request);

        ValidationResult validationResult = await validator.ValidateAsync(sanitizedRequest);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                   .Select(e => e.ErrorMessage)
                   .ToList();

            throw new CustomValidationException("Validation failed ", errors);
        }

        SendBatchMessageCommandResult result = await mediator.Send(sanitizedRequest);

        return Results.Ok(ReferenceApiResponse.SuccessResponse(referenceNumber: result.CampaignId));
    }
}
