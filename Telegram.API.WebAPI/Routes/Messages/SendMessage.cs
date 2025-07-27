using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Interfaces;
using Telegram.API.WebAPI.Models;

namespace Telegram.API.WebAPI.Routes.Messages;

public class SendMessage : IRoute<SendMessageCommand>
{
    public static async Task<IResult> RegisterRoute(
        IMediator mediator,
        IValidator<SendMessageCommand> validator,
        SendMessageCommand request)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

            throw new CustomValidationException("Validation failed " + errors);
        }

        SendMessageCommandResult response = await mediator.Send(request);

        return Results.Ok(ApiResponse<SendMessageCommandResult>.SuccessResponse(response.ReferenceNumber));
    }
}
