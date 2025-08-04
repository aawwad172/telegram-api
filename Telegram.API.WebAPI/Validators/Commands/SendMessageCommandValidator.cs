using FluentValidation;
using System.Text.RegularExpressions;
using Telegram.API.Application.CQRS.Commands;

namespace Telegram.API.WebAPI.Validators.Commands;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required.");

        RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Mobile number is required.")
            .Must(BeValidAndNormalizePhoneNumber)
            .WithMessage("Invalid phone number format.");

        RuleFor(x => x.MessageText)
            .NotEmpty().WithMessage("Message text is required.");

        RuleFor(x => x.BotKey)
            .NotEmpty()
            .MaximumLength(50)
            .MinimumLength(43)
            .WithMessage("Bot key must be between 43 and 45 characters long.");
    }

    private bool BeValidAndNormalizePhoneNumber(SendMessageCommand command, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var normalizedNumber = NormalizePhoneNumber(phoneNumber);
        if (string.IsNullOrEmpty(normalizedNumber))
            return false;

        // Update the command object with the normalized number
        command.PhoneNumber = normalizedNumber;
        return true;
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        // Remove all spaces, dashes, and other formatting characters
        var cleanNumber = Regex.Replace(phoneNumber, @"[\s\-\(\)]+", "");

        // Handle different country code formats
        if (cleanNumber.StartsWith("+962") || cleanNumber.StartsWith("00962") || cleanNumber.StartsWith("962"))
        {
            // Jordan format - normalize to 962XXXXXXXX
            return NormalizeJordanNumber(cleanNumber);
        }
        else if (cleanNumber.StartsWith("+20") || cleanNumber.StartsWith("0020") || cleanNumber.StartsWith("20"))
        {
            // Egypt format - normalize to 20XXXXXXXXX
            return NormalizeEgyptNumber(cleanNumber);
        }
        else if (cleanNumber.StartsWith("+968") || cleanNumber.StartsWith("00968") || cleanNumber.StartsWith("968"))
        {
            // Oman format - normalize to 968XXXXXXXX
            return NormalizeOmanNumber(cleanNumber);
        }
        else
        {
            // Check if it's a local number that can be auto-detected
            return DetectAndNormalizeLocalNumber(cleanNumber);
        }
    }

    private string NormalizeJordanNumber(string number)
    {
        // Remove country code prefixes
        if (number.StartsWith("+962"))
            number = number.Substring(4);
        else if (number.StartsWith("00962"))
            number = number.Substring(5);
        else if (number.StartsWith("962"))
            number = number.Substring(3);

        // Remove leading zero if present (local format)
        if (number.StartsWith("0"))
            number = number.Substring(1);

        // Validate Jordan mobile number format (should be 8 digits after removing country code and leading zero)
        if (Regex.IsMatch(number, @"^[7][0-9]{8}$")) // Jordan mobile numbers start with 7
        {
            return "962" + number;
        }

        return string.Empty; // Invalid format
    }

    private string NormalizeEgyptNumber(string number)
    {
        // Remove country code prefixes
        if (number.StartsWith("+20"))
            number = number.Substring(3);
        else if (number.StartsWith("0020"))
            number = number.Substring(4);
        else if (number.StartsWith("20"))
            number = number.Substring(2);

        // Remove leading zero if present (local format)
        if (number.StartsWith("0"))
            number = number.Substring(1);

        // Validate Egypt mobile number format (should be 10 digits after removing country code and leading zero)
        if (Regex.IsMatch(number, @"^[1][0-9]{9}$")) // Egypt mobile numbers start with 1
        {
            return "20" + number;
        }

        return string.Empty; // Invalid format
    }

    private string NormalizeOmanNumber(string number)
    {
        // Remove country code prefixes
        if (number.StartsWith("+968"))
            number = number.Substring(4);
        else if (number.StartsWith("00968"))
            number = number.Substring(5);
        else if (number.StartsWith("968"))
            number = number.Substring(3);

        // Remove leading zero if present (local format)
        if (number.StartsWith("0"))
            number = number.Substring(1);

        // Validate Oman mobile number format (should be 8 digits after removing country code and leading zero)
        if (Regex.IsMatch(number, @"^[79][0-9]{7}$")) // Oman mobile numbers start with 7 or 9
        {
            return "968" + number;
        }

        return string.Empty; // Invalid format
    }

    private string DetectAndNormalizeLocalNumber(string number)
    {
        // Remove leading zero for local number detection
        var numberWithoutZero = number.StartsWith("0") ? number.Substring(1) : number;

        // Jordan local number detection (starts with 7, 9 digits total including leading 7)
        if (Regex.IsMatch(numberWithoutZero, @"^[7][0-9]{8}$"))
        {
            return "962" + numberWithoutZero;
        }

        // Egypt local number detection (starts with 1, 10 digits total including leading 1)
        if (Regex.IsMatch(numberWithoutZero, @"^[1][0-9]{9}$"))
        {
            return "20" + numberWithoutZero;
        }

        // Oman local number detection (starts with 7 or 9, 8 digits total)
        if (Regex.IsMatch(numberWithoutZero, @"^[79][0-9]{7}$"))
        {
            return "968" + numberWithoutZero;
        }

        return string.Empty; // Could not detect country
    }
}