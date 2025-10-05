using FluentValidation;
using Telegram.API.Application.CQRS.Commands.Message;

namespace Telegram.API.WebAPI.Validators.Commands.Messages;

public class PortalSendBatchMessageCommandValidator : AbstractValidator<PortalSendBatchMessageCommand>
{
    public PortalSendBatchMessageCommandValidator()
    {
        RuleFor(x => x.BotId)
            .NotEmpty()
            .WithMessage("BotId is required.")
            .GreaterThan(0)
            .WithMessage("BotId should be greater than 0");

        RuleFor(x => x.EncryptedCustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Items cannot be empty.");

        RuleForEach(x => x.Items)
            .SetValidator(new PortalBatchMessageItemValidator());
    }
}

public class PortalBatchMessageItemValidator : AbstractValidator<BatchMessageItem>
{
    public PortalBatchMessageItemValidator()
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
            .WithMessage("MessageText is required");
    }
}

