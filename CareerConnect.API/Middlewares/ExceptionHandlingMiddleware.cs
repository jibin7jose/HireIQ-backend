using System.Net;
using System.Text.Json;
using CareerConnect.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CareerConnect.API.Middlewares;

/// <summary>
/// Catches unhandled exceptions and maps them to standardized ProblemDetails responses.
/// Registered in Program.cs before all other middleware.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Validation Failed",
                "One or more validation errors occurred.",
                ve.Errors.GroupBy(e => e.PropertyName)
                         .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            ),
            NotFoundException nfe => (
                HttpStatusCode.NotFound,
                "Not Found",
                nfe.Message,
                (Dictionary<string, string[]>?)null
            ),
            ConflictException ce => (
                HttpStatusCode.Conflict,
                "Conflict",
                ce.Message,
                (Dictionary<string, string[]>?)null
            ),
            UnauthorizedException ue => (
                HttpStatusCode.Forbidden,
                "Forbidden",
                ue.Message,
                (Dictionary<string, string[]>?)null
            ),
            DomainException de => (
                HttpStatusCode.BadRequest,
                "Bad Request",
                de.Message,
                (Dictionary<string, string[]>?)null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.",
                (Dictionary<string, string[]>?)null
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title  = title,
            Detail = detail
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
