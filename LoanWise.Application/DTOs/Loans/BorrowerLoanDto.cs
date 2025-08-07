using LoanWise.Domain.Enums;

namespace LoanWise.Application.Features.Loans.DTOs
{
    public class BorrowerLoanDto
    {
        public Guid LoanId { get; set; }
        public decimal Amount { get; set; }
        public int DurationInMonths { get; set; }
        public LoanPurpose Purpose { get; set; }
        public LoanStatus Status { get; set; }
        public decimal FundedAmount { get; set; }
        public RiskLevel RiskLevel { get; set; }
    }
}
