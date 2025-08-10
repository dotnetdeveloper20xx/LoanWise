// LoanFundedEvent.cs
using System;
using LoanWise.Domain.Common;
using MediatR;

namespace LoanWise.Domain.Events
{
    /// <summary>
    /// Raised when a funding contribution is recorded for a loan.
    /// Includes a flag indicating if the loan has become fully funded.
    /// </summary>
    public sealed record LoanFundedEvent(
        Guid LoanId,
        Guid FundingId,
        Guid LenderId,
        decimal Amount,
        bool IsFullyFunded
    ) : IDomainEvent, INotification;
    
}
