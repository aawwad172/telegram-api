using FluentValidation;
using Telegram.API.Application.CQRS.Commands;

namespace Telegram.API.WebAPI.Validators.Commands.Messages;

public class SendBatchMessagesCommandValidator : AbstractValidator<SendBatchMessagesCommand>
{
    public SendBatchMessagesCommandValidator()
    {
        RuleFor(x => x.BotId)
            .NotEmpty()
            .WithMessage("BotId is required.")
            .GreaterThan(0)
            .WithMessage("BotId should be greater than 0");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Items cannot be empty.");

        RuleForEach(x => x.Items)
            .SetValidator(new BatchMessageItemValidator());
    }
}

public class BatchMessageItemValidator : AbstractValidator<BatchMessageItem>
{
    public BatchMessageItemValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^\+?[0-9]+$")
            .WithMessage("Phone number must start with '+' optionally and contain digits only.")
            .MaximumLength(20)
            .WithMessage("Phone number cannot exceed 20 digits.");

        RuleFor(x => x.MessageText)
            .NotEmpty()
            .WithMessage("Message text is required.")
            .MaximumLength(4096)
            .WithMessage("Message text cannot exceed 4096 characters.");
    }
}
