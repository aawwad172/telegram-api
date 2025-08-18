using FluentValidation;
using Telegram.API.Application.CQRS.Commands;

namespace Telegram.API.WebAPI.Validators.Commands;

public class SendBatchMessagesCommandValidator : AbstractValidator<SendBatchMessagesCommand>
{
    public SendBatchMessagesCommandValidator()
    {
        RuleFor(x => x.BotKey)
            .NotEmpty()
            .WithMessage("BotKey is required.");
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Items cannot be empty.");

    }
}
