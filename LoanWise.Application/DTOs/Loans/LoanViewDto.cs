using System;
using System.Collections.Generic;
using LoanWise.Domain.Enums;
using LoanWise.Application.DTOs.Fundings;
using LoanWise.Application.DTOs.Repayments;

namespace LoanWise.Application.DTOs.Loans
{
    /// <summary>
    /// Represents a loan's data returned to clients for viewing.
    /// </summary>
    public class LoanViewDto
    {
        /// <summary>
        /// Unique identifier for the loan.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Display name of the borrower.
        /// </summary>
        public string BorrowerName { get; set; } = default!;

        /// <summary>
        /// Total loan amount requested.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Loan duration in months.
        /// </summary>
        public int DurationInMonths { get; set; }

        /// <summary>
        /// Reason category for the loan.
        /// </summary>
        public LoanPurpose Purpose { get; set; }

        /// <summary>
        /// Current status of the loan.
        /// </summary>
        public LoanStatus Status { get; set; }

        /// <summary>
        /// Assigned risk level of the loan.
        /// </summary>
        public RiskLevel RiskLevel { get; set; }

        /// <summary>
        /// How much funding has been received so far.
        /// </summary>
        public decimal AmountFunded { get; set; }

        /// <summary>
        /// Whether the loan is fully funded.
        /// </summary>
        public bool IsFullyFunded => AmountFunded >= Amount;

        /// <summary>
        /// Date when the loan was submitted.
        /// </summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// List of fundings contributed to this loan.
        /// </summary>
        public List<FundingDto> Fundings { get; set; } = new();

        /// <summary>
        /// List of repayment entries for the loan.
        /// </summary>
        public List<RepaymentDto> Repayments { get; set; } = new();
    }
}
