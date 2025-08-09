using LoanWise.Domain.Common;

namespace LoanWise.Domain.Events
{
    public sealed record RepaymentDueEvent(Guid LoanId, Guid BorrowerId, Guid RepaymentId, DateTime DueDate, decimal Amount) : IDomainEvent;

}
