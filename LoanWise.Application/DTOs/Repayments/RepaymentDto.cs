using System;

namespace LoanWise.Application.DTOs.Repayments
{
    /// <summary>
    /// Represents a repayment installment in a loan schedule.
    /// </summary>
    public sealed class RepaymentDto
    {
        public Guid Id { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAtUtc { get; set; }
        public bool IsOverdue { get; set; }
        public string Status { get; set; } = "Scheduled"; // Paid | Overdue | Scheduled
    }
}
