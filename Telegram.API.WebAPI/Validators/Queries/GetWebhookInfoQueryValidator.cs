using FluentValidation;
using Telegram.API.Application.CQRS.Queries.Bots;

namespace Telegram.API.WebAPI.Validators.Queries;

public class GetWebhookInfoQueryValidator : AbstractValidator<GetWebhookInfoQuery>
{
    public GetWebhookInfoQueryValidator()
    {
        RuleFor(x => x.Username)
           .NotEmpty()
           .WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");

        RuleFor(x => x.BotKey)
            .NotEmpty()
            .MaximumLength(50)
            .MinimumLength(43)
            .WithMessage("Bot key must be between 43 and 50 characters long.");
    }
}
