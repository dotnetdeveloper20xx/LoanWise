namespace LoanWise.Application.DTOs.Loans
{
    public sealed record BorrowerLoanHistoryDto(
          Guid LoanId,
          decimal LoanAmount,
          decimal TotalFunded,
          string Purpose,
          string Status,
          string RiskLevel,
          DateTime CreatedAtUtc,
          DateTime? ApprovedAtUtc,
          DateTime? DisbursedAtUtc
      );
}
