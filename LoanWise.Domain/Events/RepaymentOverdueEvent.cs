using LoanWise.Domain.Common;
using MediatR;

namespace LoanWise.Domain.Events
{
    /// <summary>Raised when a scheduled repayment becomes overdue.</summary>
    public sealed record RepaymentOverdueEvent(
        Guid LoanId,
        Guid RepaymentId,
        DateTime DueDate
    ) : IDomainEvent, INotification
    {
        public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    }
}
