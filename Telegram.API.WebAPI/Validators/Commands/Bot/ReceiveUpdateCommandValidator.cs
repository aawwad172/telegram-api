using FluentValidation;
using Telegram.API.Application.CQRS.Commands.Bots;

namespace Telegram.API.WebAPI.Validators.Commands.Bot;

public class ReceiveUpdateCommandValidator : AbstractValidator<ReceiveUpdateCommand>
{
    public ReceiveUpdateCommandValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty()
            .WithMessage("PublicId is required");

        RuleFor(x => x.SecretToken)
            .NotEmpty()
            .WithMessage("SecretToken is required");

        RuleFor(x => x.Update)
            .NotEmpty()
            .WithMessage("Update is required");
    }
}
