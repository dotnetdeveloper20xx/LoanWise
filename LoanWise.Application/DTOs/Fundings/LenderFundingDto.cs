namespace LoanWise.Application.Features.Fundings.DTOs
{
    public sealed record LenderFundingDto(
    Guid LoanId,
    decimal LoanAmount,
    decimal TotalFunded,
    string Purpose,
    string Status,
    decimal AmountFundedByYou
);

}
