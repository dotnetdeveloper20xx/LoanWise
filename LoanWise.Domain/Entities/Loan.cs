using System;
using System.Collections.Generic;
using LoanWise.Domain.Enums;
using LoanWise.Domain.ValueObjects;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a loan application made by a borrower.
    /// </summary>
    public class Loan
    {
        private readonly List<Funding> _fundings = new();
        private readonly List<Repayment> _repayments = new();

        /// <summary>
        /// Unique identifier for the loan.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Foreign key to the borrower who applied for this loan.
        /// </summary>
        public Guid BorrowerId { get; private set; }

        /// <summary>
        /// Navigation property to the borrower user.
        /// </summary>
        public User Borrower { get; private set; }

        /// <summary>
        /// Total amount of the loan requested.
        /// </summary>
        public Money Amount { get; private set; }

        /// <summary>
        /// Duration of the loan in months.
        /// </summary>
        public int DurationInMonths { get; private set; }

        /// <summary>
        /// Purpose of the loan (e.g., Education, Medical).
        /// </summary>
        public LoanPurpose Purpose { get; private set; }

        /// <summary>
        /// Current status of the loan (e.g., Pending, Approved).
        /// </summary>
        public LoanStatus Status { get; private set; }

        /// <summary>
        /// Risk level assigned to this loan (Low, Medium, High).
        /// </summary>
        public RiskLevel RiskLevel { get; private set; }

        /// <summary>
        /// Navigation property to fundings from lenders.
        /// </summary>
        public IReadOnlyCollection<Funding> Fundings => _fundings.AsReadOnly();

        /// <summary>
        /// Navigation property to scheduled and paid repayments.
        /// </summary>
        public IReadOnlyCollection<Repayment> Repayments => _repayments.AsReadOnly();

        /// <summary>
        /// Required by EF Core.
        /// </summary>
        private Loan() { }

        /// <summary>
        /// Creates a new loan application instance.
        /// </summary>
        public Loan(Guid id, Guid borrowerId, Money amount, int durationInMonths, LoanPurpose purpose)
        {
            Id = id;
            BorrowerId = borrowerId;
            Amount = amount;
            DurationInMonths = durationInMonths;
            Purpose = purpose;
            Status = LoanStatus.Pending;
            RiskLevel = RiskLevel.Unknown;
        }

        /// <summary>
        /// Approves the loan with a calculated risk level.
        /// </summary>
        public void Approve(RiskLevel riskLevel)
        {
            if (Status != LoanStatus.Pending)
                throw new InvalidOperationException("Only pending loans can be approved.");

            RiskLevel = riskLevel;
            Status = LoanStatus.Approved;
        }

        /// <summary>
        /// Rejects the loan.
        /// </summary>
        public void Reject()
        {
            if (Status != LoanStatus.Pending)
                throw new InvalidOperationException("Only pending loans can be rejected.");

            Status = LoanStatus.Rejected;
        }

        /// <summary>
        /// Adds a lender funding record.
        /// </summary>
        public void AddFunding(Funding funding)
        {
            _fundings.Add(funding);
        }

        /// <summary>
        /// Adds a repayment to the schedule.
        /// </summary>
        public void AddRepayment(Repayment repayment)
        {
            _repayments.Add(repayment);
        }

        /// <summary>
        /// Checks if the loan is fully funded.
        /// </summary>
        public bool IsFullyFunded()
        {
            decimal total = 0;
            foreach (var funding in _fundings)
            {
                total += funding.Amount.Value;
            }

            return total >= Amount.Value;
        }
    }
}
