using Telegram.API.Application.CQRS.Commands;

namespace Telegram.API.Application.HelperServices;

public class CommandsSanitizer
{
    public static SendMessageCommand SanitizeSendMessageCommand(SendMessageCommand command)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command), "Command cannot be null");
        }
        // Ensure all required properties are set without leading or trailing whitespace
        command.Username = command.Username.Trim();
        command.Password = command.Password.Trim();
        command.PhoneNumber = command.PhoneNumber.Trim();
        command.MessageText = command.MessageText.Trim();
        command.BotKey = command.BotKey.Trim();

        // Remove leading '+' and '0' from PhoneNumber
        command.PhoneNumber.Trim('+');
        command.PhoneNumber.TrimStart('0');
        return command;
    }
}
