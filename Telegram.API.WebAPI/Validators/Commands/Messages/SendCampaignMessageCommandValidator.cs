using FluentValidation;
using Telegram.API.Application.CQRS.Commands;

namespace Telegram.API.WebAPI.Validators.Commands.Messages;

public class SendCampaignMessageCommandValidator : AbstractValidator<SendCampaignMessageCommand>
{
    public SendCampaignMessageCommandValidator()
    {
        RuleFor(x => x.BotId)
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

        RuleForEach(x => x.Items)
            .SetValidator(new CampaignMessageItemValidator());
    }
}

public class CampaignMessageItemValidator : AbstractValidator<CampaignMessageItem>
{
    public CampaignMessageItemValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^\+?[0-9]+$")
            .WithMessage("Phone number must start with '+' optionally and contain digits only.")
            .MaximumLength(20)
            .WithMessage("Phone number cannot exceed 20 digits.");
    }
}
