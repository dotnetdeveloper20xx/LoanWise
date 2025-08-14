namespace LoanWise.Application.DTOs.Repayments
{
    public sealed class LoanRepaymentSummaryDto
    {
        public Guid LoanId { get; set; }
        public decimal LoanAmount { get; set; }
        public int DurationInMonths { get; set; }

        public int InstallmentsGenerated { get; set; }
        public int InstallmentsPaid { get; set; }
        public decimal InstallmentAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Remaining { get; set; }
        public DateTime? NextDueDate { get; set; }

        public List<RepaymentDto> Repayments { get; set; } = new();
    }
}
