using System;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a simulated or imported credit profile for a user.
    /// </summary>
    public class CreditProfile
    {
        /// <summary>
        /// Primary key tied directly to the user.
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Navigation property to the associated user.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Mock or actual numeric credit score.
        /// </summary>
        public int CreditScore { get; private set; }

        /// <summary>
        /// Risk tier derived from the credit score.
        /// </summary>
        public RiskTier Tier { get; private set; }

        /// <summary>
        /// Date and time when this score was last evaluated.
        /// </summary>
        public DateTime CalculatedAtUtc { get; private set; }

        /// <summary>
        /// Optional provider note or scoring method.
        /// </summary>
        public string? SourceNote { get; private set; }

        /// <summary>
        /// Required by EF Core.
        /// </summary>
        private CreditProfile() { }

        /// <summary>
        /// Creates a new credit profile record for a user.
        /// </summary>
        /// <param name="userId">User ID to associate.</param>
        /// <param name="creditScore">Credit score value (e.g., 700).</param>
        /// <param name="tier">Mapped risk tier (Low, Medium, High).</param>
        /// <param name="calculatedAtUtc">Timestamp of calculation.</param>
        /// <param name="sourceNote">Optional description or provider info.</param>
        public CreditProfile(
            Guid userId,
            int creditScore,
            RiskTier tier,
            DateTime calculatedAtUtc,
            string? sourceNote = null)
        {
            if (creditScore < 0)
                throw new ArgumentOutOfRangeException(nameof(creditScore), "Credit score cannot be negative.");

            UserId = userId;
            CreditScore = creditScore;
            Tier = tier;
            CalculatedAtUtc = calculatedAtUtc;
            SourceNote = sourceNote;
        }
    }

    /// <summary>
    /// Represents simplified risk bands derived from credit score.
    /// </summary>
    public enum RiskTier
    {
        Unknown = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }
}
