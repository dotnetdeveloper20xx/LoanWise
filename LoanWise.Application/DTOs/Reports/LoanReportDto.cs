namespace LoanWise.Application.DTOs.Reports
{
    public sealed record LoanReportDto(
         Guid LoanId,
         string BorrowerName,
         decimal LoanAmount,
         string Purpose,
         string Status,
         string RiskLevel,
         decimal TotalFunded,
         DateTime CreatedAtUtc,
         DateTime? ApprovedAtUtc,
         DateTime? DisbursedAtUtc
     );
}
