using System.Net;
using System.Text.Json;
using Escale.API.DTOs.Common;
using FluentValidation;

namespace Escale.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                validationEx.Errors.Select(e => e.ErrorMessage).ToList()
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                (List<string>?)null
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                exception.Message,
                (List<string>?)null
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                exception.Message,
                (List<string>?)null
            ),
            ArgumentException => (
                HttpStatusCode.BadRequest,
                exception.Message,
                (List<string>?)null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred",
                (List<string>?)null
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.ErrorResponse(message, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = null });
        await context.Response.WriteAsync(json);
    }
}
