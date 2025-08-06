namespace LoanWise.Domain.Enums
{
    /// <summary>
    /// Represents the intended use or category of a loan.
    /// </summary>
    public enum LoanPurpose
    {
        /// <summary>
        /// Educational expenses like tuition, courses, or materials.
        /// </summary>
        Education = 0,

        /// <summary>
        /// Home renovation, repair, or improvement work.
        /// </summary>
        HomeImprovement = 1,

        /// <summary>
        /// Medical bills or emergency healthcare needs.
        /// </summary>
        Medical = 2,

        /// <summary>
        /// Funding for starting or growing a business.
        /// </summary>
        Business = 3,

        /// <summary>
        /// Consolidation of existing debt into one repayment.
        /// </summary>
        DebtConsolidation = 4,

        /// <summary>
        /// Any other use case not explicitly listed.
        /// </summary>
        Other = 5
    }
}
