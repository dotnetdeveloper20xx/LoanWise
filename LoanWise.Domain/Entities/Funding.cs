using System;
using LoanWise.Domain.ValueObjects;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a lender's funding contribution to a specific loan.
    /// </summary>
    public class Funding
    {
        /// <summary>
        /// Unique identifier for the funding transaction.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Foreign key reference to the lender (User).
        /// </summary>
        public Guid LenderId { get; private set; }

        /// <summary>
        /// Navigation property to the lender.
        /// </summary>
        public User Lender { get; private set; }

        /// <summary>
        /// Foreign key reference to the loan being funded.
        /// </summary>
        public Guid LoanId { get; private set; }

        /// <summary>
        /// Navigation property to the target loan.
        /// </summary>
        public Loan Loan { get; private set; }

        /// <summary>
        /// Amount of money contributed by the lender.
        /// </summary>
        public Money Amount { get; private set; }

        /// <summary>
        /// Date and time when the funding was made.
        /// </summary>
        public DateTime FundedOn { get; private set; }

        /// <summary>
        /// Required by EF Core.
        /// </summary>
        private Funding() { }

        /// <summary>
        /// Creates a new lender funding entry.
        /// </summary>
        /// <param name="id">Unique funding ID.</param>
        /// <param name="lenderId">User ID of the lender.</param>
        /// <param name="loanId">Loan ID being funded.</param>
        /// <param name="amount">Funding amount.</param>
        /// <param name="fundedOn">Timestamp of funding.</param>
        public Funding(Guid id, Guid lenderId, Guid loanId, Money amount, DateTime fundedOn)
        {
            if (amount.Value <= 0)
                throw new ArgumentException("Funding amount must be greater than zero.", nameof(amount));

            Id = id;
            LenderId = lenderId;
            LoanId = loanId;
            Amount = amount;
            FundedOn = fundedOn;
        }
    }
}
