using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoanWise.Application.Behaviors
{
    /// <summary>
    /// Pipeline behavior that executes all registered FluentValidation validators before the request handler.
    /// </summary>
    /// <typeparam name="TRequest">The command or query request type.</typeparam>
    /// <typeparam name="TResponse">The expected response type.</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Count > 0)
                {
                    var failureMessages = failures
                        .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
                        .ToList();

                    throw new ValidationException("Validation failed", failures);
                }
            }

            return await next();
        }
    }
}
