namespace LoanWise.Application.DTOs.Reports
{
    public sealed record FundingReportDto(
       Guid FundingId,
       Guid LoanId,
       string BorrowerName,
       Guid LenderId,
       string LenderName,
       decimal AmountFunded,
       DateTime FundedOn,
       string LoanStatus
   );
}
