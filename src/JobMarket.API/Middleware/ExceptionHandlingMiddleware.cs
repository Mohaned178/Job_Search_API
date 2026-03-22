using System.Diagnostics;
using System.Net;
using System.Text.Json;
using JobMarket.Domain.Exceptions;
using FluentValidation;

namespace JobMarket.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        string traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        (HttpStatusCode statusCode, object body) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, (object)new
            {
                type = "ValidationError",
                traceId,
                errors = ve.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            }),
            DomainException de => (HttpStatusCode.BadRequest, new
            {
                type = "DomainError",
                traceId,
                message = de.Message
            }),
            NotFoundException nfe => (HttpStatusCode.NotFound, new
            {
                type = "NotFound",
                traceId,
                message = nfe.Message
            }),
            UnauthorizedException ue => (HttpStatusCode.Forbidden, new
            {
                type = "Forbidden",
                traceId,
                message = ue.Message
            }),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, new
            {
                type = "Unauthorized",
                traceId,
                message = "Authentication is required to access this resource."
            }),
            _ => (HttpStatusCode.InternalServerError, new
            {
                type = "ServerError",
                traceId,
                message = "An unexpected error occurred."
            })
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception — TraceId: {TraceId}", traceId);
        else
            _logger.LogWarning(exception, "Handled exception {ExceptionType} — TraceId: {TraceId}", exception.GetType().Name, traceId);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
