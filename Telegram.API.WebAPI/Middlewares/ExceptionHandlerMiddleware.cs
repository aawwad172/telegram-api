using System.Text.Json;
using A2ASerilog;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Models;

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
            Console.WriteLine($"NotFoundException {ex.Message}");
            LoggerService.Warning("NotFoundException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-5", "NOT_FOUND", StatusCodes.Status404NotFound);
        }
        catch (EnvironmentVariableNotSetException ex)
        {
            Console.WriteLine($"EnvironmentVariableNotSetException {ex.Message}");
            LoggerService.Warning("EnvironmentVariableNotSetException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-99", "INTERNAL_SERVER_ERROR", StatusCodes.Status500InternalServerError);
        }
        catch (UnauthenticatedException ex)
        {
            Console.WriteLine($"UnauthenticatedException {ex.Message}");
            LoggerService.Warning("UnauthenticatedException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-2", "UNAUTHENTICATED", StatusCodes.Status401Unauthorized);
        }
        catch (UnauthorizedException ex)
        {
            Console.WriteLine($"UnauthorizedException {ex.Message}");
            LoggerService.Warning("UnauthorizedException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-2", "UNAUTHORIZED", StatusCodes.Status403Forbidden);
        }
        catch (ConflictException ex)
        {
            Console.WriteLine($"ConflictException {ex.Message}");
            LoggerService.Warning("ConflictException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-10", "CONFLICT", StatusCodes.Status409Conflict);
        }
        catch (CustomValidationException ex)
        {
            string joined = JoinErrors(ex.Errors);
            Console.WriteLine($"CustomValidationException {ex.Message} {joined}");
            LoggerService.Warning("CustomValidationException {Message} {Errors}", ex.Message, joined);
            await HandleExceptionAsync(context, "-1", joined, StatusCodes.Status400BadRequest);
        }
        catch (TelegramApiException ex)
        {
            Console.WriteLine($"Telegram API Exception: {ex.Message}");
            LoggerService.Error("TelegramApiException {Message}", ex.Message);

            await HandleExceptionAsync(context, "-32", "TELEGRAM_EXCEPTION", StatusCodes.Status502BadGateway);
        }
        catch (BadRequestException ex)
        {
            System.Console.WriteLine($"BadRequestException {ex.Message}");
            LoggerService.Error("BadRequestException {Message}", ex.Message);

            await HandleExceptionAsync(context, "-33", "BAD_REQUEST", StatusCodes.Status400BadRequest);
        }
        catch (InvalidPhoneNumberException ex)
        {
            Console.WriteLine($"InvalidPhoneNumberException {ex.Message}");
            LoggerService.Warning("InvalidPhoneNumberException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-30", "INVALID_PHONE_NUMBER", StatusCodes.Status400BadRequest);
        }
        catch (ChatIdNotFoundException ex)
        {
            Console.WriteLine($"ChatId Not Found Exception {ex.Message}");
            LoggerService.Warning("ChatIdNotFoundException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-31", "CHAT_ID_NOT_FOUND", StatusCodes.Status404NotFound);
        }
        catch (EmptyMessagesPackageException ex)
        {
            Console.WriteLine($"Empty Messages Package Exception {ex.Message}");
            LoggerService.Warning("EmptyMessagesPackageException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-40", "EMPTY_MESSAGES_BATCH", StatusCodes.Status400BadRequest);
        }
        catch (CouldNotDeleteFileException ex)
        {
            Console.WriteLine($"Couldn't Delete FileException {ex.Message}");
            LoggerService.Error("Couldn'tDeleteFileException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-50", "COULD_NOT_DELETE_FILE", StatusCodes.Status500InternalServerError);
        }
        catch (InvalidBotKeyException ex)
        {
            Console.WriteLine($"InvalidBotKeyException {ex.Message}");
            LoggerService.Warning("InvalidBotKeyException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-32", "INVALID_BOT_KEY", StatusCodes.Status400BadRequest);
        }
        catch (BotIsNotActiveException ex)
        {
            Console.WriteLine($"BotIsNotActiveException {ex.Message}");
            LoggerService.Error("BotIsNotActiveException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-33", "BOT_KEY_NOT_ACTIVE", StatusCodes.Status403Forbidden);
        }
        catch (DatabaseException ex)
        {
            Console.WriteLine($"DatabaseException {ex.Message}");
            LoggerService.Error("DatabaseException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-20", "DATABASE_ERROR", StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UnhandledException {ex}");
            LoggerService.Error("UnhandledException {Message}", ex.Message);
            await HandleExceptionAsync(context, "-99", "UNEXPECTED_ERROR_OCCURRED", StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string errorCode, string message, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        ApiResponse<object> response = ApiResponse<object>.ErrorResponse(errorMessage: message, errorCode: errorCode);
        string result = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(result);
    }

    private string JoinErrors(IEnumerable<string> errors) => string.Join(", ", errors);
}
