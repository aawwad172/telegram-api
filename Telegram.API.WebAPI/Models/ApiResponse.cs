namespace Telegram.API.WebAPI.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T Data { get; init; }
    public string Message { get; init; }
    public string ErrorCode { get; init; }

    private ApiResponse(
        bool success,
        T response,
        string message,
        string errorCode)
    {
        Success = success;
        Message = message;
        ErrorCode = errorCode;
        Data = response;
    }

    // Factory methods
    public static ApiResponse<T> SuccessResponse(T response)
        => new ApiResponse<T>(success: true, response: response, message: "Success", errorCode: "0");

    public static ApiResponse<T> ErrorResponse(T response, string errorMessage, string errorCode)
        => new ApiResponse<T>(success: false, response: response, message: errorMessage, errorCode: errorCode);
}