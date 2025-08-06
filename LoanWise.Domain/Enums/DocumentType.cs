namespace LoanWise.Domain.Enums
{
    /// <summary>
    /// Represents the type of document uploaded by a user for verification or KYC purposes.
    /// </summary>
    public enum DocumentType
    {
        /// <summary>
        /// A payslip from an employer.
        /// </summary>
        Payslip = 0,

        /// <summary>
        /// Government-issued ID card (driver’s license, national ID).
        /// </summary>
        ID = 1,

        /// <summary>
        /// Passport document.
        /// </summary>
        Passport = 2,

        /// <summary>
        /// Recent bank statement.
        /// </summary>
        BankStatement = 3,

        /// <summary>
        /// Utility bill (electricity, water, gas, etc.).
        /// </summary>
        UtilityBill = 4,

        /// <summary>
        /// Any other document type not categorized above.
        /// </summary>
        Other = 5
    }
}
