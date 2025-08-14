using FluentValidation;
using Microsoft.EntityFrameworkCore;                // DbUpdateConcurrencyException
using StoreBoost.Application.Common.Models;
using System.Net;
// If you have your own NotFound/BadRequest exceptions, use those instead of SendGrid ones.
// using LoanWise.Application.Common.Exceptions;

namespace LoanWise.Api.Middleware
{
    /// <summary>
    /// Global exception handler returning consistent ApiResponse JSON with proper HTTP codes.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            // ── Known, user-facing errors ─────────────────────────────────────────────
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed");
                var message = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed.";
                await WriteAsync(context, HttpStatusCode.BadRequest, message);
            }
            catch (InvalidOperationException ex)
            {
                // Domain rule violations (e.g., "Funding can only be added to approved/funded/disbursed loans.")
                _logger.LogWarning(ex, "Domain rule violation");
                await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found");
                await WriteAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Forbidden");
                await WriteAsync(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict");
                await WriteAsync(context, (HttpStatusCode)409, "Conflict: resource was modified by another process. Please retry.");
            }
            // ── Fallback ─────────────────────────────────────────────────────────────
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteAsync(context, HttpStatusCode.InternalServerError, "Something went wrong. Please try again later.");
            }
        }

        private static async Task WriteAsync(HttpContext context, HttpStatusCode status, string message)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)status;
                var payload = ApiResponse<object>.FailureResult(message);
                await context.Response.WriteAsJsonAsync(payload);
            }
        }
    }
}
