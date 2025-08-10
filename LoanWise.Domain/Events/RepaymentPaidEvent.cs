// RepaymentPaidEvent.cs
using System;
using LoanWise.Domain.Common;
using MediatR;

namespace LoanWise.Domain.Events
{
    /// <summary>
    /// Raised when a specific repayment is marked as paid.
    /// </summary>
    public sealed record RepaymentPaidEvent(
        Guid LoanId,
        Guid RepaymentId,
        DateTime PaidOn
    ) : IDomainEvent, INotification
    {
        public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    }
}
