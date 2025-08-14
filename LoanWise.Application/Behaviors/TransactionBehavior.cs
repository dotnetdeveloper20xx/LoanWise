// LoanWise.Application/Behaviors/TransactionBehavior.cs
using LoanWise.Application.Abstractions;
using MediatR;

namespace LoanWise.Application.Behaviors
{
    public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IUnitOfWork _uow;

        public TransactionBehavior(IUnitOfWork uow) => _uow = uow;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var isCommand = typeof(TRequest).Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase);
            if (!isCommand) return await next();

            await _uow.BeginTransactionAsync(ct);
            try
            {
                var res = await next();
                await _uow.CommitTransactionAsync(ct);
                return res;
            }
            catch
            {
                await _uow.RollbackTransactionAsync(ct);
                throw;
            }
        }
    }
}
