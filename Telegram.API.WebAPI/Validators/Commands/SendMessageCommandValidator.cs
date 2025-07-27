using FluentValidation;
using Telegram.API.Application.CQRS.Commands;

namespace Telegram.API.WebAPI.Validators.Commands;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required.");

        RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Mobile number is required.")
            .Matches(@"^\+[1-9]\d{1,14}$")
            .WithMessage("Mobile number must be in E.164 format (e.g. +12345678901).");


        RuleFor(x => x.MessageText)
            .NotEmpty().WithMessage("Message text is required.")
            .MaximumLength(500)
            .WithMessage("Message text cannot exceed 500 characters.");
    }
}
