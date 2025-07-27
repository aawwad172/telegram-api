namespace Telegram.API.WebAPI.Models;

public class ApiResponse<T>
{
    private bool Success { get; init; }
    private string? ErrorMessage { get; init; }
    private string? ErrorCode { get; init; }
    private string? ReferenceNumber { get; init; }

    private ApiResponse(
        bool success,
        string? referenceNumber,
        string? errorMessage,
        string? errorCode)
    {
        Success = success;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        ReferenceNumber = referenceNumber;
    }

    // Factory methods
    public static ApiResponse<T> SuccessResponse(string referenceNumber)
        => new ApiResponse<T>(success: true, referenceNumber: referenceNumber, errorMessage: null, errorCode: null);

    public static ApiResponse<T> ErrorResponse(string? referenceNumber, string errorMessage, string errorCode)
        => new ApiResponse<T>(success: false, referenceNumber: referenceNumber, errorMessage: errorMessage, errorCode: errorCode);
}