using LoanWise.Domain.Common;

namespace LoanWise.Domain.Events
{
    public sealed record RepaymentPaidEvent(Guid LoanId, Guid BorrowerId, Guid RepaymentId, decimal Amount) : IDomainEvent;

}
