using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.API.Application.CQRS.Commands.Bots;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Interfaces;
using Telegram.API.WebAPI.Models;

namespace Telegram.API.WebAPI.Routes.Bot;

public class RegisterBot : ICommandRoute<RegisterBotCommand>
{
    public static async Task<IResult> RegisterRoute(
        [FromBody] RegisterBotCommand request,
        [FromServices] IMediator mediator,
        [FromServices] FluentValidation.IValidator<RegisterBotCommand> validator)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                   .Select(e => e.ErrorMessage)
                   .ToList();

            throw new CustomValidationException("Validation failed ", errors);
        }

        var result = await mediator.Send(request);

        return Results.Ok(ApiResponse<RegisterBotCommandResult>.CreatedResponse(result));
    }
}
