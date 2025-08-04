using System.Security.Cryptography;

namespace Telegram.API.WebAPI.Models;

public class ApiResponse
{
    public bool Success { get; init; }
    public string RefNumber { get; init; }
    public string ErrorMessage { get; init; }
    public string ErrorCode { get; init; }

    private ApiResponse(
        bool success,
        string refNumber,
        string errorMessage,
        string errorCode)
    {
        Success = success;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        RefNumber = refNumber;
    }

    // Factory methods
    public static ApiResponse SuccessResponse(string refNumber)
        => new(success: true, refNumber: refNumber, errorMessage: "Success", errorCode: "0");

    public static ApiResponse ErrorResponse(string errorMessage, string errorCode)
        => new(success: false, refNumber: "", errorMessage: errorMessage, errorCode: errorCode);
}