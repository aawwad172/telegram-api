using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Interfaces;
using Telegram.API.WebAPI.Models;

namespace Telegram.API.WebAPI.Routes.Messages;

public class SendBatchMessage : ICommandRoute<SendBatchMessagesCommand>
{
    public static async Task<IResult> RegisterRoute(
        [FromBody] SendBatchMessagesCommand request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<SendBatchMessagesCommand> validator)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                   .Select(e => e.ErrorMessage)
                   .ToList();

            throw new CustomValidationException("Validation failed ", errors);
        }

        SendBatchMessageCommandResult result = await mediator.Send(request);

        return Results.Ok(ApiResponse<SendBatchMessageCommandResult>.SuccessResponse(result));
    }
}
