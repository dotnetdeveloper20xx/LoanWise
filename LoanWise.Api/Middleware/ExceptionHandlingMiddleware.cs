// LoanWise.API/Middlewares/ExceptionHandlingMiddleware.cs
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LoanWise.API.Middlewares
{
    public sealed class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) => _logger = logger;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                var problem = CreateProblemDetails(context, ex);
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }

        private static ProblemDetails CreateProblemDetails(HttpContext ctx, Exception ex)
        {
            var traceId = ctx.TraceIdentifier;

            return ex switch
            {
                UnauthorizedAccessException => new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = ex.Message,
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://httpstatuses.com/401",
                    Extensions = { ["traceId"] = traceId }
                },
                KeyNotFoundException => new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://httpstatuses.com/404",
                    Extensions = { ["traceId"] = traceId }
                },
                FluentValidation.ValidationException v => new ValidationProblemDetails(
                    v.Errors.GroupBy(e => e.PropertyName)
                            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://httpstatuses.com/400",
                    Extensions = { ["traceId"] = traceId }
                },
                _ => new ProblemDetails
                {
                    Title = "Unexpected error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://httpstatuses.com/500",
                    Extensions = { ["traceId"] = traceId }
                }
            };
        }
    }
}
