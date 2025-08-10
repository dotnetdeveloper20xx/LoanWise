namespace LoanWise.Domain.Entities;

public class BorrowerRiskSnapshot
{
    public Guid BorrowerId { get; set; }                  // PK
    public int CreditScore { get; set; }                  // e.g., 550–850
    public string RiskTier { get; set; } = "Unknown";     // Low | Medium | High | Unknown
    public string KycStatus { get; set; } = "Unknown";    // Pending | Verified | Failed | Unknown
    public string FlagsJson { get; set; } = "[]";         // JSON array of strings
    public DateTime? LastVerifiedAtUtc { get; set; }      // only set by VerifyKycCommand
    public DateTime LastScoreAtUtc { get; set; }          // when the score was generated
    public DateTime UpdatedAtUtc { get; set; }            // snapshot upsert time
}
