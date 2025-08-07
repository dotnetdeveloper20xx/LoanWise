
using LoanWise.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Errors.Model;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">Logger instance for logging errors.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to handle any exceptions thrown by the pipeline.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
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
        /// <param name="context">HTTP context.</param>
        /// <param name="statusCode">HTTP status code to return.</param>
        /// <param name="message">Error message to send in the response.</param>
        private async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<Guid>.Fail(message);
            var json = System.Text.Json.JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}
