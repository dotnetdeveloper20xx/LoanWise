using LoanWise.Domain.Common;
using MediatR;
using System;

namespace LoanWise.Domain.Events
{
    /// <summary>
    /// Raised when a loan becomes fully funded.
    /// </summary>
    public class LoanFundedEvent : IDomainEvent, INotification
    {
        public Guid LoanId { get; }
        public Guid BorrowerId { get; }
        public decimal AmountFunded { get; }
        public DateTime OccurredOnUtc { get; }

        public LoanFundedEvent(Guid loanId, Guid borrowerId, decimal amountFunded)
        {
            LoanId = loanId;
            BorrowerId = borrowerId;
            AmountFunded = amountFunded;
            OccurredOnUtc = DateTime.UtcNow;
        }
    }
}
