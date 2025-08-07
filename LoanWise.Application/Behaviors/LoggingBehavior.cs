using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LoanWise.Application.Behaviors
{
    /// <summary>
    /// Logs the execution of all MediatR requests, including start and end time.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation("➡️ Handling {RequestName}", requestName);

            try
            {
                var response = await next();
                _logger.LogInformation("✅ {RequestName} handled successfully", requestName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ {RequestName} failed with exception", requestName);
                throw;
            }
        }
    }
}
