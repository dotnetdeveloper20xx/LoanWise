using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Common;
using MediatR;

namespace LoanWise.Infrastructure.Common
{
    public sealed class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;
        public DomainEventDispatcher(IMediator mediator) => _mediator = mediator;

        public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct)
        {
            foreach (var e in events)
                await _mediator.Publish(e, ct);
        }
    }
}
