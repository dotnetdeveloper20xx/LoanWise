namespace LoanWise.Domain.Enums
{
    /// <summary>
    /// Represents the type of action performed within the escrow system.
    /// </summary>
    public enum EscrowTransactionType
    {
        /// <summary>
        /// Funds added to escrow from a lender.
        /// </summary>
        Deposit = 0,

        /// <summary>
        /// Funds released to the borrower after full funding and approval.
        /// </summary>
        Release = 1,

        /// <summary>
        /// Funds refunded to the lender (e.g., loan rejected or cancelled).
        /// </summary>
        Refund = 2
    }
}
