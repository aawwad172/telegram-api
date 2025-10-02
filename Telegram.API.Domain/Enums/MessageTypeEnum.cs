namespace Telegram.API.Domain.Enums;

public enum MessageTypeEnum
{
    A, // High Priority Single Message that represent API
    AC, // Campaign Message from the API
    AF, // Batch Message from the API
    C, // Campaign Message from the Portal
    CF, // Batch Message from the Portal (Campaign File)
}
