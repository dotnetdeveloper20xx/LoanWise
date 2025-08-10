namespace LoanWise.Application.DTOs.Admin;

public sealed class BorrowerKycListItemDto
{
    public Guid BorrowerId { get; set; }
    public string BorrowerName { get; set; } = default!;
    public string KycStatus { get; set; } = default!;         // Verified | Pending | Failed | Unknown
    public int CreditScore { get; set; }
    public string RiskTier { get; set; } = default!;          // Low | Medium | High | Unknown
    public DateTime? LastVerifiedAtUtc { get; set; }
    public DateTime LastScoreAtUtc { get; set; }
    public string[] Flags { get; set; } = Array.Empty<string>();
}
