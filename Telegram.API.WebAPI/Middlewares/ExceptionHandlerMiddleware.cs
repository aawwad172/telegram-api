using A2ASerilog;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Models;
using System.Text.Json;


namespace Telegram.API.WebAPI.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Warning($"NotFoundException occurred: {ex.Message}");
            await HandleExceptionAsync(context, "-5", "NOT_FOUND", StatusCodes.Status404NotFound);
        }
        catch (EnvironmentVariableNotSetException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Warning($"EnvironmentVariableNotSetException occurred: {ex.Message}");
            await HandleExceptionAsync(context, "INTERNAL_SERVER_ERROR", ex.Message, StatusCodes.Status500InternalServerError);
        }
        catch (UnauthenticatedException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Warning($"UnauthenticatedException occurred: {ex.Message}");
            await HandleExceptionAsync(context, "-2", "UNAUTHENTICATED", StatusCodes.Status401Unauthorized);
        }
        catch (UnauthorizedException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Warning($"UnauthorizedException occurred: {ex.Message}");
            await HandleExceptionAsync(context, "-2", "UNAUTHORIZED", StatusCodes.Status403Forbidden);
        }
        catch (ConflictException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Warning($"ConflictException occurred: {ex.Message}");
            await HandleExceptionAsync(context, "-10", "CONFLICT", StatusCodes.Status409Conflict);
        }
        catch (CustomValidationException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Warning($"CustomValidationException occurred: {ex.Message}: {JoinErrors(ex.Errors)}");
            await HandleExceptionAsync(
                context: context,
                errorCode: "-1",
                message: JoinErrors(ex.Errors),
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidPhoneNumberException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Warning($"InvalidPhoneNumberException occurred: {ex.Message}");
            await HandleExceptionAsync(context, "-30", "INVALID_PHONE_NUMBER", StatusCodes.Status400BadRequest);
        }
        catch (ChatIdNotFoundException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Warning($"ChatIdNotFoundException occurred: {ex.Message}");
            await HandleExceptionAsync(context, "-31", "CHAT_ID_NOT_FOUND", StatusCodes.Status404NotFound);
        }
        catch (EmptyMessagesPackageException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Warning($"EmptyMessagesBatchException occurred: {ex.Message}");
            await HandleExceptionAsync(context, "-40", "EMPTY_MESSAGES_BATCH", StatusCodes.Status400BadRequest);
        }
        catch (DatabaseException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Error($"DatabaseException occurred: {ex.Message}");
            await HandleExceptionAsync(context, "-20", "DATABASE_ERROR", StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LoggerService.Error($"An unexpected error occurred: {ex.Message}");
            await HandleExceptionAsync(context, "-99", "An unexpected error occurred.", StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string errorCode, string message, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        // Json Serialization Options to make all camelCase
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true // Optional: for pretty printing
        };

        ApiResponse<object> response = ApiResponse<object>.ErrorResponse(errorMessage: message, errorCode: errorCode);
        string result = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(result);
    }

    private string JoinErrors(IEnumerable<string> errors) => string.Join(", ", errors);
}