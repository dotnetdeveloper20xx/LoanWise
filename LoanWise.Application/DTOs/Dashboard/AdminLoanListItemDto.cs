using LoanWise.Domain.Enums;

namespace LoanWise.Application.DTOs.Dashboard
{
    public class AdminLoanListItemDto
    {
        public Guid Id { get; set; }
        public Guid BorrowerId { get; set; }
        public decimal Amount { get; set; }
        public int DurationInMonths { get; set; }
        public LoanPurpose Purpose { get; set; }
        public LoanStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
