using A2ASerilog;
using Telegram.API.Domain.Exceptions;
using Telegram.API.WebAPI.Models;
using System.ComponentModel.DataAnnotations;
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
            LoggerService.Warning("NotFoundException occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, "NOT_FOUND", ex.Message, StatusCodes.Status404NotFound);
        }
        catch (EnvironmentVariableNotSetException ex)
        {
            LoggerService.Warning("EnvironmentVariableNotSetException occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, "ENV_VAR_MISSING", ex.Message, StatusCodes.Status500InternalServerError);
        }
        catch (ValidationException ex)
        {
            LoggerService.Warning("ValidationException occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, "VALIDATION_ERROR", ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (UnauthenticatedException ex)
        {
            LoggerService.Warning("UnauthenticatedException occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, "UNAUTHENTICATED", ex.Message, StatusCodes.Status401Unauthorized);
        }
        catch (UnauthorizedException ex)
        {
            LoggerService.Warning("UnauthorizedException occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, "UNAUTHORIZED", ex.Message, StatusCodes.Status403Forbidden);
        }
        catch (ConflictException ex)
        {
            LoggerService.Warning("ConflictException occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, "CONFLICT", ex.Message, StatusCodes.Status409Conflict);
        }
        catch (CustomValidationException ex)
        {
            LoggerService.Warning("CustomValidationException occurred: {Message}", ex.Message);
            await HandleExceptionAsync(
                context,
                "VALIDATION_ERROR",
                JoinErrors(ex.Errors),
                StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            LoggerService.Error("An unexpected error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, "UNEXPECTED_ERROR", "An unexpected error occurred.", StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string errorCode, string message, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        ApiResponse<string> response = ApiResponse<string>.ErrorResponse(errorMessage: message, referenceNumber: null, errorCode: statusCode.ToString());
        string result = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(result);
    }

    private string JoinErrors(IEnumerable<string> errors)
    {
        return string.Join(", ", errors);
    }
}