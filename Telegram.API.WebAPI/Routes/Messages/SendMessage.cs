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

public class SendMessage : ICommandRoute<SendMessageCommand>
{
    public static async Task<IResult> RegisterRoute(
        [FromBody] SendMessageCommand request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<SendMessageCommand> validator)
    {
        SendMessageCommand sanitizedRequest = CommandsSanitizer.Sanitize(request);
        ValidationResult validationResult = await validator.ValidateAsync(sanitizedRequest);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

            throw new CustomValidationException("Validation failed ", errors);
        }

        SendMessageCommandResult result = await mediator.Send(sanitizedRequest);

        // Use this to prevent extra memory allocation
        return Results.Ok(ReferenceApiResponse.SuccessResponse(referenceNumber: result.ReferenceNumber));
    }
}
