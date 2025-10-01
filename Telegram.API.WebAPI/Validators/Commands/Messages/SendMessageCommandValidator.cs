using FluentValidation;
using Telegram.API.Application.CQRS.Commands.Message;

namespace Telegram.API.WebAPI.Validators.Commands.Messages;

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
            .Matches(@"^\+?[0-9]+$")
            .WithMessage("Phone number must start with '+' optionally and contain digits only.");


        RuleFor(x => x.MessageText)
            .NotEmpty()
            .WithMessage("Message text is required.")
            .MaximumLength(4096)
            .WithMessage("Message text cannot exceed 4096 characters.");

        RuleFor(x => x.BotId)
            .NotEmpty()
            .WithMessage("BotId is required.")
            .GreaterThan(0)
            .WithMessage("BotId should be greater than 0");
    }
}