using LoanWise.Domain.Common;

namespace LoanWise.Domain.Events
{
    public sealed record FundingAddedEvent(Guid LoanId, Guid LenderId, decimal Amount) : IDomainEvent;

}
