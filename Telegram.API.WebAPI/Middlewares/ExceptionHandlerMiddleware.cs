using System.Text.Json;
using A2ASMS.Utility.Logger;
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
            A2ALogger.Warning($"NotFoundException {ex.Message}");
            await HandleExceptionAsync(context, $"-5$", $"NOT_FOUND$", StatusCodes.Status404NotFound);
        }
        catch (EnvironmentVariableNotSetException ex)
        {
            Console.WriteLine($"EnvironmentVariableNotSetException {ex.Message}");
            A2ALogger.Warning($"EnvironmentVariableNotSetException {ex.Message}");
            await HandleExceptionAsync(context, $"-99$", $"INTERNAL_SERVER_ERROR$", StatusCodes.Status500InternalServerError);
        }
        catch (UnauthenticatedException ex)
        {
            Console.WriteLine($"UnauthenticatedException {ex.Message}");
            A2ALogger.Warning($"UnauthenticatedException {ex.Message}");
            await HandleExceptionAsync(context, $"-2$", $"UNAUTHENTICATED$", StatusCodes.Status401Unauthorized);
        }
        catch (UnauthorizedException ex)
        {
            Console.WriteLine($"UnauthorizedException {ex.Message}");
            A2ALogger.Warning($"UnauthorizedException {ex.Message}");
            await HandleExceptionAsync(context, $"-2$", $"UNAUTHORIZED$", StatusCodes.Status403Forbidden);
        }
        catch (ConflictException ex)
        {
            Console.WriteLine($"ConflictException {ex.Message}");
            A2ALogger.Warning($"ConflictException {ex.Message}");
            await HandleExceptionAsync(context, $"-10$", $"CONFLICT$", StatusCodes.Status409Conflict);
        }
        catch (CustomValidationException ex)
        {
            string joined = JoinErrors(ex.Errors);
            Console.WriteLine($"CustomValidationException {ex.Message} {joined}");
            A2ALogger.Warning($"CustomValidationException {ex.Message} {joined}");
            await HandleExceptionAsync(context, $"-1$", joined, StatusCodes.Status400BadRequest);
        }
        catch (TelegramApiException ex)
        {
            Console.WriteLine($"Telegram API Exception: {ex.Message}");
            A2ALogger.Error($"TelegramApiException {ex.Message}");

            await HandleExceptionAsync(context, $"-32$", $"TELEGRAM_EXCEPTION$", StatusCodes.Status502BadGateway);
        }
        catch (BadRequestException ex)
        {
            System.Console.WriteLine($"BadRequestException {ex.Message}");
            A2ALogger.Error($"BadRequestException {ex.Message}");

            await HandleExceptionAsync(context, $"-33$", $"BAD_REQUEST$", StatusCodes.Status400BadRequest);
        }
        catch (InvalidPhoneNumberException ex)
        {
            Console.WriteLine($"InvalidPhoneNumberException {ex.Message}");
            A2ALogger.Warning($"InvalidPhoneNumberException {ex.Message}");
            await HandleExceptionAsync(context, $"-30$", $"INVALID_PHONE_NUMBER$", StatusCodes.Status400BadRequest);
        }
        catch (ChatIdNotFoundException ex)
        {
            Console.WriteLine($"ChatId Not Found Exception {ex.Message}");
            A2ALogger.Warning($"ChatIdNotFoundException {ex.Message}");
            await HandleExceptionAsync(context, $"-31$", $"CHAT_ID_NOT_FOUND$", StatusCodes.Status404NotFound);
        }
        catch (EmptyMessagesPackageException ex)
        {
            Console.WriteLine($"Empty Messages Package Exception {ex.Message}");
            A2ALogger.Warning($"EmptyMessagesPackageException {ex.Message}");
            await HandleExceptionAsync(context, $"-40$", $"EMPTY_MESSAGES_BATCH$", StatusCodes.Status400BadRequest);
        }
        catch (CouldNotDeleteFileException ex)
        {
            Console.WriteLine($"Couldn't Delete FileException {ex.Message}");
            A2ALogger.Error($"Couldn'tDeleteFileException {ex.Message}");
            await HandleExceptionAsync(context, $"-50$", $"COULD_NOT_DELETE_FILE$", StatusCodes.Status500InternalServerError);
        }
        catch (InvalidBotKeyException ex)
        {
            Console.WriteLine($"InvalidBotKeyException {ex.Message}");
            A2ALogger.Warning($"InvalidBotKeyException {ex.Message}");
            await HandleExceptionAsync(context, $"-32$", $"INVALID_BOT_KEY$", StatusCodes.Status400BadRequest);
        }
        catch (BotIsNotActiveException ex)
        {
            Console.WriteLine($"BotIsNotActiveException {ex.Message}");
            A2ALogger.Error($"BotIsNotActiveException {ex.Message}");
            await HandleExceptionAsync(context, $"-33$", $"BOT_KEY_NOT_ACTIVE$", StatusCodes.Status403Forbidden);
        }
        catch (DatabaseException ex)
        {
            Console.WriteLine($"DatabaseException {ex.Message}");
            A2ALogger.Error($"DatabaseException {ex.Message}");
            await HandleExceptionAsync(context, $"-20$", $"DATABASE_ERROR$", StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UnhandledException {ex}");
            A2ALogger.Error($"UnhandledException {ex.Message}");
            await HandleExceptionAsync(context, $"-99$", $"UNEXPECTED_ERROR_OCCURRED$", StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string errorCode, string message, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = $"application/json$";

        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        ApiResponse<object> response = ApiResponse<object>.ErrorResponse(errorMessage: message, errorCode: errorCode);
        string result = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(result);
    }

    private string JoinErrors(IEnumerable<string> errors) => string.Join($", $", errors);
}
