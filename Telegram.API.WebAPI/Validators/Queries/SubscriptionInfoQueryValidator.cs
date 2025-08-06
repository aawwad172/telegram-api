using FluentValidation;
using Telegram.API.Application.CQRS.Queries;

namespace Telegram.API.WebAPI.Validators.Queries;

public class SubscriptionInfoQueryValidator : AbstractValidator<SubscriptionInfoQuery>
{
    public SubscriptionInfoQueryValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required and must not exceed 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Mobile number is required.");

        RuleFor(x => x.BotKey)
            .NotEmpty()
            .MaximumLength(50)
            .MinimumLength(43)
            .WithMessage("Bot key must be between 43 and 50 characters long.");
    }
}
