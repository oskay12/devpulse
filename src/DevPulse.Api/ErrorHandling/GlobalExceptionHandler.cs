using System.Diagnostics;
using DevPulse.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Api.ErrorHandling;

/// <summary>
/// Converts unhandled exceptions into RFC 7807 problem responses.
/// </summary>
/// <remarks>
/// Registered via <c>AddExceptionHandler</c> and invoked by
/// <c>app.UseExceptionHandler()</c>, which sits first in the pipeline so it also
/// covers failures raised inside other middleware.
/// </remarks>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private const string ProblemTypeBase = "https://devpulse.dev/problems/";

    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // A cancelled request is the client hanging up, not a server fault. There
        // is nobody left to write a response to, so skip it entirely.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        var problem = Map(exception);
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        problem.Extensions["traceId"] = traceId;
        problem.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";

        Log(exception, problem.Status ?? StatusCodes.Status500InternalServerError, traceId);

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem
        });
    }

    private static ProblemDetails Map(Exception exception) => exception switch
    {
        NotFoundException ex => Problem(
            StatusCodes.Status404NotFound, "Resource not found", "not-found", ex.Message),

        ConflictException ex => Problem(
            StatusCodes.Status409Conflict, "Conflict", "conflict", ex.Message),

        DomainValidationException ex => Validation(ex),

        WebhookAuthenticationException => Problem(
            StatusCodes.Status401Unauthorized,
            "Webhook authentication failed",
            "webhook-unauthorized",
            // Deliberately vague: see WebhookAuthenticationException remarks.
            "The webhook signature could not be verified."),

        DependencyUnavailableException ex => Problem(
            StatusCodes.Status503ServiceUnavailable,
            "Dependency unavailable",
            "dependency-unavailable",
            $"{ex.DependencyName} is currently unreachable. Please retry."),

        DbUpdateConcurrencyException => Problem(
            StatusCodes.Status409Conflict,
            "Concurrent modification",
            "concurrency",
            "The record was modified by another request. Reload and try again."),

        BadHttpRequestException ex => Problem(
            StatusCodes.Status400BadRequest, "Malformed request", "malformed-request", ex.Message),

        // Anything unrecognised is a bug. Return no detail — the traceId is the
        // only handle the caller gets, and the full exception goes to the logs.
        _ => Problem(
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred",
            "internal-error",
            "An unexpected error occurred. Quote the traceId when reporting this.")
    };

    private static ProblemDetails Validation(DomainValidationException exception)
    {
        var problem = Problem(
            StatusCodes.Status400BadRequest, "Validation failed", "validation-error", exception.Message);

        if (exception.Errors is { Count: > 0 })
        {
            problem.Extensions["errors"] = exception.Errors;
        }

        return problem;
    }

    private static ProblemDetails Problem(int status, string title, string type, string detail) => new()
    {
        Status = status,
        Title = title,
        Type = ProblemTypeBase + type,
        Detail = detail
    };

    private void Log(Exception exception, int status, string traceId)
    {
        // 5xx means we broke; 4xx means the caller did. Only the former deserves
        // an error-level entry and a full stack trace.
        if (status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception returning {StatusCode}. TraceId={TraceId}",
                status,
                traceId);
        }
        else
        {
            _logger.LogWarning(
                "Request failed with {StatusCode}: {ExceptionType} — {Message}. TraceId={TraceId}",
                status,
                exception.GetType().Name,
                exception.Message,
                traceId);
        }
    }
}
