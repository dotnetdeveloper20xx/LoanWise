// LoanDisbursedEvent.cs
using System;
using LoanWise.Domain.Common;
using MediatR;

namespace LoanWise.Domain.Events
{
    /// <summary>
    /// Raised when a loan is disbursed and funds are released to the borrower.
    /// </summary>
    public sealed record LoanDisbursedEvent(
        Guid LoanId,
        DateTime DisbursedOn
    ) : IDomainEvent, INotification
    {
        public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    }
}
