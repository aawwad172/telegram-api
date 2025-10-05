using Telegram.API.Application.CQRS.Commands.Message;
using Telegram.API.Domain.Exceptions;

namespace Telegram.API.Application.Utilities;

public class CommandsSanitizer
{
    public static SendBatchMessagesCommand Sanitize(SendBatchMessagesCommand command)
    {
        return command with
        {
            Username = command.Username?.Trim() ?? string.Empty,
            Password = command.Password?.Trim() ?? string.Empty,
            Items = command.Items?.Select(i => i with
            {
                PhoneNumber = NormalizeOrThrow(i.PhoneNumber),
                MessageText = i.MessageText?.Trim() ?? string.Empty
            }).ToList() ?? []
        };
    }

    public static SendMessageCommand Sanitize(SendMessageCommand command)
    {
        return command with
        {
            Username = command.Username?.Trim() ?? string.Empty,
            Password = command.Password?.Trim() ?? string.Empty,
            PhoneNumber = NormalizeOrThrow(command.PhoneNumber),
            MessageText = command.MessageText?.Trim() ?? string.Empty
        };
    }

    public static SendCampaignMessageCommand Sanitize(SendCampaignMessageCommand command)
    {
        return command with
        {
            Username = command.Username?.Trim() ?? string.Empty,
            Password = command.Password?.Trim() ?? string.Empty,
            MessageText = command.MessageText.Trim() ?? string.Empty,
            Items = command.Items?.Select(i => i with
            {
                PhoneNumber = NormalizeOrThrow(i.PhoneNumber),
            }).ToList() ?? []
        };
    }

    public static PortalSendCampaignMessageCommand Sanitize(PortalSendCampaignMessageCommand command)
    {
        return command with
        {
            MessageText = command.MessageText.Trim() ?? string.Empty,
            Items = command.Items?.Select(i => i with
            {
                PhoneNumber = NormalizeOrThrow(i.PhoneNumber),
            }).ToList() ?? []
        };
    }

    public static PortalSendBatchMessageCommand Sanitize(PortalSendBatchMessageCommand command)
    {
        return command with
        {
            Items = command.Items?.Select(i => i with
            {
                PhoneNumber = NormalizeOrThrow(i.PhoneNumber),
                MessageText = i.MessageText?.Trim() ?? string.Empty
            }).ToList() ?? []
        };
    }

    // 🔧 Shared helper
    private static string NormalizeOrThrow(string raw)
    {
        if (!CommandSanitizerHelpers.TryNormalizePhoneNumber(raw, out string? normalized))
            throw new InvalidPhoneNumberException($"Unable to normalize phone number '{raw}'");

        return normalized;
    }
}
