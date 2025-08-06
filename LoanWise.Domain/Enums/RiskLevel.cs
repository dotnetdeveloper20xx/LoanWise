namespace LoanWise.Domain.Enums
{
    /// <summary>
    /// Represents the assessed financial risk associated with a loan or borrower.
    /// </summary>
    public enum RiskLevel
    {
        /// <summary>
        /// Risk level has not yet been determined.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Low risk: excellent credit history, stable income.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Medium risk: average credit history, stable employment.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// High risk: poor credit, unstable income, high debt ratio.
        /// </summary>
        High = 3
    }
}
