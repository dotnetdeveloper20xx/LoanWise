// LoanWise.Application/DTOs/Borrowers/BorrowerRiskSummaryDto.cs
namespace LoanWise.Application.DTOs.Borrowers;

public sealed class BorrowerRiskSummaryDto
{
    public Guid BorrowerId { get; set; }
    public int CreditScore { get; set; }                // 550–850
    public string RiskTier { get; set; } = default!;    // Low | Medium | High
    public string KycStatus { get; set; } = default!;   // Pending | Verified | Failed | Unknown
    public string[] Flags { get; set; } = Array.Empty<string>();
    public DateTime GeneratedAtUtc { get; set; }
}
