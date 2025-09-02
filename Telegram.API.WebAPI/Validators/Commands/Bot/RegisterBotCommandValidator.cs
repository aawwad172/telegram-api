using FluentValidation;
using Telegram.API.Application.CQRS.Commands.Bots;

namespace Telegram.API.WebAPI.Validators.Commands.Bot;

public class RegisterBotCommandValidator : AbstractValidator<RegisterBotCommand>
{
    public RegisterBotCommandValidator()
    {
        RuleFor(x => x.BotKey)
        .NotEmpty()
        .WithMessage("BotKey is required.")
        .MaximumLength(50)
        .WithMessage("Bot key should not exceed 50 characters");


        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}
