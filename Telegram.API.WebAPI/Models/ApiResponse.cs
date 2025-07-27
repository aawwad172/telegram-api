namespace Telegram.API.WebAPI.Models;

public class ApiResponse<T>
{
    private bool Success { get; init; }
    private int StatusCode { get; init; }
    private string? ErrorMessage { get; init; }
    private string? ErrorCode { get; init; }
    private string? ReferenceNumber { get; init; }

    private ApiResponse(
        bool success,
        string? referenceNumber,
        int statusCode,
        string? errorMessage,
        string? errorCode)
    {
        Success = success;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        ReferenceNumber = referenceNumber;
    }

    // Factory methods
    public static ApiResponse<T> SuccessResponse(string referenceNumber, int statusCode = 200)
        => new ApiResponse<T>(true, referenceNumber, statusCode, null, null);

    public static ApiResponse<T> ErrorResponse(string errorMessage, string errorCode, int statusCode)
        => new ApiResponse<T>(false, null, statusCode, errorMessage, errorCode);
}