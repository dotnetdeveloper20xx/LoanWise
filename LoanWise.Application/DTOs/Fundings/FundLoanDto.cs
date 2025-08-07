namespace LoanWise.Application.Features.Fundings.DTOs
{
    /// <summary>
    /// Request body for funding a loan.
    /// </summary>
    public class FundLoanDto
    {
        public Guid LenderId { get; set; }
        public decimal Amount { get; set; }
    }
}
