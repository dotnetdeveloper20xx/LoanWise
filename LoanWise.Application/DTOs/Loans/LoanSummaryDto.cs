using LoanWise.Domain.Enums;
using LoanWise.Domain.ValueObjects;

namespace LoanWise.Application.Features.Loans.DTOs
{
    public class LoanSummaryDto
    {
        public Guid LoanId { get; set; }
        public Guid BorrowerId { get; set; }
        public decimal Amount { get; set; }
        public decimal FundedAmount { get; set; }
        public int DurationInMonths { get; set; }
        public LoanPurpose Purpose { get; set; }
        public LoanStatus Status { get; set; }
    }
}
