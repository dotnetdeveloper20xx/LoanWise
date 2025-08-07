using System;
using System.ComponentModel.DataAnnotations;
using LoanWise.Domain.Enums;

namespace LoanWise.Application.DTOs.Loans
{
    /// <summary>
    /// Represents the incoming request to apply for a new loan.
    /// </summary>
    public class ApplyLoanRequestDto
    {
        /// <summary>
        /// Total loan amount requested.
        /// </summary>
        [Required]
        [Range(100, 1_000_000, ErrorMessage = "Amount must be between 100 and 1,000,000.")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Duration of the loan in months.
        /// </summary>
        [Required]
        [Range(1, 120, ErrorMessage = "Duration must be between 1 and 120 months.")]
        public int DurationInMonths { get; set; }

        /// <summary>
        /// Purpose category of the loan.
        /// </summary>
        [Required]
        public LoanPurpose Purpose { get; set; }

        /// <summary>
        /// Optional description for loan reason/details.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// (Optional) Monthly income for eligibility scoring.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? MonthlyIncome { get; set; }
    }
}
