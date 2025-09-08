using FluentValidation;
using Telegram.API.Application.CQRS.Commands.Bots;

namespace Telegram.API.WebAPI.Validators.Commands.Bot;

public class ReceiveUpdateCommandValidator : AbstractValidator<ReceiveUpdateCommand>
{
    public ReceiveUpdateCommandValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty().WithMessage("PublicId is required.")
            .Length(32).WithMessage("PublicId must be a 32-char hex string."); // adjust if GUID with dashes

        RuleFor(x => x.SecretToken)
            .NotEmpty().WithMessage("SecretToken is required.");

        RuleFor(x => x.Update)
            .NotNull().WithMessage("Update is required.");

        // Optional: require minimum message structure for your flow
        When(x => x.Update is not null, () =>
        {
            RuleFor(x => x.Update!.Message)
                .NotNull().WithMessage("Update.message is required.");

            RuleFor(x => x.Update!.Message!.Chat)
                .NotNull().WithMessage("Update.message.chat is required.");
        });
    }
}
