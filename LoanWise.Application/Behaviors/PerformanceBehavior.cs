using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LoanWise.Application.Behaviors
{
    /// <summary>
    /// Measures the execution time of each request and logs if it exceeds a threshold.
    /// </summary>
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

        private const int WarningThresholdMilliseconds = 500; // Customizable

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestName = typeof(TRequest).Name;

            var response = await next();

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > WarningThresholdMilliseconds)
            {
                _logger.LogWarning(
                    "LONG RUNNING REQUEST: {RequestName} took {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds
                );
            }
            else
            {
                _logger.LogDebug(
                    "{RequestName} executed in {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds
                );
            }

            return response;
        }
    }
}
