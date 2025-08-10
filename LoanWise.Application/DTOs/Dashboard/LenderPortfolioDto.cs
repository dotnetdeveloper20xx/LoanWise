namespace LoanWise.Application.DTOs.Dashboard
{
    /// <summary>
    /// Summary of a lender's portfolio including funding, returns, and outstanding balance.
    /// </summary>
    public class LenderPortfolioDto
    {
        /// <summary>
        /// Total amount the lender has funded across all loans.
        /// </summary>
        public decimal TotalFunded { get; set; }

        /// <summary>
        /// Number of distinct loans the lender has funded.
        /// </summary>
        public int NumberOfLoansFunded { get; set; }

        /// <summary>
        /// Total amount received back from repayments (proportional to their funding).
        /// </summary>
        public decimal TotalReturned { get; set; }

        /// <summary>
        /// Remaining amount still outstanding in active loans.
        /// </summary>
        public decimal OutstandingBalance { get; set; }
    }
}
