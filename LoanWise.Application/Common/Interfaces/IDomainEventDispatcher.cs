using LoanWise.Domain.Common;

namespace LoanWise.Application.Common.Interfaces
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents);
    }
}
