using System.Net;
using System.Text.Json;
using ArtemisBankingPro.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebAPI.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            ValidationException validationEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation error",
                Detail = string.Join(" | ", validationEx.Errors.Select(e => e.ErrorMessage)),
                Type = "https://tools.ietf.org/html/rfc7807"
            },
            HighRiskClientException highRiskEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Conflict,
                Title = "High risk client",
                Detail = highRiskEx.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            },
            NotFoundException notFoundEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Resource not found",
                Detail = notFoundEx.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            },
            DomainException domainEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Business rule violation",
                Detail = domainEx.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            },
            UnauthorizedAccessException => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "Unauthorized",
                Detail = exception.Message,
                Type = "https://tools.ietf.org/html/rfc7807"
            },
            _ => new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An unexpected error occurred",
                Detail = "Please try again later or contact support.",
                Type = "https://tools.ietf.org/html/rfc7807"
            }
        };

        if (problemDetails.Status == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning("Handled exception: {Message}", exception.Message);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? 500;

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}