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
        (HttpStatusCode statusCode, object body) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, (object)new
            {
                type = "ValidationError",
                errors = ve.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            }),
            DomainException de => (HttpStatusCode.BadRequest, new
            {
                type = "DomainError",
                message = de.Message
            }),
            NotFoundException nfe => (HttpStatusCode.NotFound, new
            {
                type = "NotFound",
                message = nfe.Message
            }),
            UnauthorizedException ue => (HttpStatusCode.Forbidden, new
            {
                type = "Forbidden",
                message = ue.Message
            }),
            _ => (HttpStatusCode.InternalServerError, new
            {
                type = "ServerError",
                message = "An unexpected error occurred."
            })
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
