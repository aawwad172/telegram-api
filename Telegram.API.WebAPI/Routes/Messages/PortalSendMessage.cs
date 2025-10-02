using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.API.Application.CQRS.Commands.Message;
using Telegram.API.WebAPI.Interfaces;

namespace Telegram.API.WebAPI.Routes.Messages;

public class PortalSendMessage : ICommandRoute<PortalSendCampaignCommand>
{
    public static Task<IResult> RegisterRoute(
        [FromBody] PortalSendCampaignCommand request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<PortalSendCampaignCommand> validator)
    {
        throw new NotImplementedException();
    }
}
