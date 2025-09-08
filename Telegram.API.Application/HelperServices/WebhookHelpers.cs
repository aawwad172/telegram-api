using Telegram.API.Application.CQRS.Commands.Bots;
using Telegram.API.Domain.Entities.Bot;

namespace Telegram.API.Application.HelperServices;

public class WebhookHelpers
{
    public static bool IsAuthorized(ReceiveUpdateCommand request, Bot bot)
    {
        // Allow if no secret configured (dev), else compare exact match
        if (string.IsNullOrWhiteSpace(bot.WebhookSecret))
            return true;

        return string.Equals(bot.WebhookSecret, request.SecretToken, StringComparison.Ordinal);
    }
}
