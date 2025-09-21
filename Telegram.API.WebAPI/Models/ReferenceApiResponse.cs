
namespace Telegram.API.WebAPI.Models;

public class ReferenceApiResponse
{
    public bool Success { get; init; }
    public string ReferenceNumber { get; init; }
    public string ErrorMessage { get; init; }
    public string ErrorCode { get; init; }

    private ReferenceApiResponse(
        bool success,
        string referenceNumber,
        string errorMessage,
        string errorCode)
    {
        Success = success;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        ReferenceNumber = referenceNumber;
    }

    // Factory methods
    public static ReferenceApiResponse SuccessResponse(string referenceNumber)
        => new(success: true, referenceNumber: referenceNumber, errorMessage: "Success", errorCode: "0");

    public static ReferenceApiResponse ErrorResponse(string errorMessage, string errorCode)
        => new(success: false, referenceNumber: default!, errorMessage: errorMessage, errorCode: errorCode);

    public static ReferenceApiResponse CreatedResponse(string referenceNumber)
        => new(success: true, referenceNumber: referenceNumber, errorMessage: "Created", errorCode: "0");
}