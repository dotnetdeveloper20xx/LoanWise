using System;

namespace LoanWise.Application.DTOs.Fundings
{
    /// <summary>
    /// Represents a funding contribution made by a lender to a loan.
    /// </summary>
    public class FundingDto
    {
        /// <summary>
        /// Unique identifier for this funding transaction.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name or display identifier of the lender.
        /// </summary>
        public string LenderName { get; set; } = default!;

        /// <summary>
        /// Amount of money contributed.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Date and time when the funding occurred (UTC).
        /// </summary>
        public DateTime FundedOn { get; set; }
    }
}
