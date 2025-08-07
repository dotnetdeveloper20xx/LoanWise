using System;
using LoanWise.Domain.Enums;

namespace LoanWise.Application.DTOs.Users
{
    /// <summary>
    /// Represents a user's credit profile with score and tier.
    /// </summary>
    public class CreditProfileDto
    {
        /// <summary>
        /// User ID that this credit profile belongs to.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The actual credit score (e.g., simulated or imported).
        /// </summary>
        public int CreditScore { get; set; }

        /// <summary>
        /// Risk tier based on score (Low, Medium, High).
        /// </summary>
        public RiskTier Tier { get; set; }

        /// <summary>
        /// Timestamp of the last time the score was calculated.
        /// </summary>
        public DateTime CalculatedAtUtc { get; set; }

        /// <summary>
        /// Optional metadata about where the score came from (simulated, mock provider, etc.).
        /// </summary>
        public string? SourceNote { get; set; }
    }
}
