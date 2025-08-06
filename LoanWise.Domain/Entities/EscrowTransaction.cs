using System;
using LoanWise.Domain.ValueObjects;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a transaction recorded in the virtual escrow system for a loan.
    /// </summary>
    public class EscrowTransaction
    {
        /// <summary>
        /// Unique identifier for the escrow transaction.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Foreign key referencing the associated loan.
        /// </summary>
        public Guid LoanId { get; private set; }

        /// <summary>
        /// Navigation property to the associated loan.
        /// </summary>
        public Loan Loan { get; private set; }

        /// <summary>
        /// Amount involved in the transaction.
        /// </summary>
        public Money Amount { get; private set; }

        /// <summary>
        /// Type of transaction: Funding deposit, release to borrower, etc.
        /// </summary>
        public EscrowTransactionType Type { get; private set; }

        /// <summary>
        /// Timestamp of when the transaction occurred.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Optional note or reference for auditing or traceability.
        /// </summary>
        public string? ReferenceNote { get; private set; }

        /// <summary>
        /// Required by EF Core.
        /// </summary>
        private EscrowTransaction() { }

        /// <summary>
        /// Creates a new escrow transaction record.
        /// </summary>
        /// <param name="id">Unique transaction ID.</param>
        /// <param name="loanId">Loan associated with the escrow entry.</param>
        /// <param name="amount">Transaction amount.</param>
        /// <param name="type">Type of escrow movement.</param>
        /// <param name="timestamp">When the transaction occurred.</param>
        /// <param name="referenceNote">Optional metadata or note.</param>
        public EscrowTransaction(
            Guid id,
            Guid loanId,
            Money amount,
            EscrowTransactionType type,
            DateTime timestamp,
            string? referenceNote = null)
        {
            if (amount.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Escrow amount must be greater than zero.");

            Id = id;
            LoanId = loanId;
            Amount = amount;
            Type = type;
            Timestamp = timestamp;
            ReferenceNote = referenceNote;
        }
    }

    /// <summary>
    /// Enumerates the types of escrow transaction activities.
    /// </summary>
    public enum EscrowTransactionType
    {
        /// <summary>
        /// Funds contributed to the escrow pool by a lender.
        /// </summary>
        Deposit = 0,

        /// <summary>
        /// Funds released to the borrower after full funding.
        /// </summary>
        Release = 1,

        /// <summary>
        /// Refund returned to lender (e.g., rejected loan).
        /// </summary>
        Refund = 2
    }
}
