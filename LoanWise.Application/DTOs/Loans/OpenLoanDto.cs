namespace LoanWise.Application.DTOs.Loans
{
    public sealed record OpenLoanDto(
       Guid Id,
       decimal Amount,
       decimal FundedTotal,
       decimal Remaining,
       string Purpose,
       string RiskLevel
   );
}
