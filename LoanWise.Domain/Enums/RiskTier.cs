namespace LoanWise.Domain.Enums
{
    /// <summary>
    /// Represents a simplified risk band derived from a user's credit score.
    /// </summary>
    public enum RiskTier
    {
        /// <summary>
        /// Risk tier is unknown or has not yet been evaluated.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Low risk: excellent score, high reliability.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Medium risk: moderate credit score, acceptable risk.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// High risk: low score, likely to default or miss payments.
        /// </summary>
        High = 3
    }
}
