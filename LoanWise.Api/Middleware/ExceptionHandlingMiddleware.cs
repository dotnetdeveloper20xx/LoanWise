
using SendGrid.Helpers.Errors.Model;
using StoreBoost.Application.Common.Models;
using System.Net;

namespace LoanWise.Api.Middleware
{
    /// <summary>
    /// Middleware that handles unhandled exceptions globally and returns structured error responses.
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
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found: {Message}", ex.Message);
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning(ex, "Bad request: {Message}", ex.Message);
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed: {Message}", ex.Message);
                var message = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed.";
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Something went wrong. Please try again later.");
            }
        }

        /// <summary>
        /// Writes a structured API error response.
        /// </summary>
        private async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.FailureResult(message);
            var json = System.Text.Json.JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}
