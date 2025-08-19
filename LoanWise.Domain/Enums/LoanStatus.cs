namespace LoanWise.Domain.Enums
{
    /// <summary>
    /// Represents the current status of a loan in its lifecycle.
    /// </summary>
    public enum LoanStatus
    {
        /// <summary>
        /// Loan has been created but not yet reviewed by admin.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Loan has been approved by an admin.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Loan has been rejected by an admin.
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// Loan has received 100% of its funding goal.
        /// </summary>
        Funded = 3,

        /// <summary>
        /// Funds have been disbursed to the borrower.
        /// </summary>
        Disbursed = 4,

        /// <summary>
        /// All repayments have been made and the loan is complete.
        /// </summary>
        Completed = 5,

        Cancelled = 6
    }
}
