using LoanWise.Domain.Enums;

namespace LoanWise.Application.Features.Fundings.DTOs
{
    public class LenderFundingDto
    {
        public Guid LoanId { get; set; }
        public decimal AmountFundedByYou { get; set; }
        public decimal TotalFunded { get; set; }
        public decimal LoanAmount { get; set; }
        public LoanPurpose Purpose { get; set; }
        public LoanStatus Status { get; set; }
    }
}
