using System;

namespace LoanWise.Application.DTOs.Repayments
{
    /// <summary>
    /// Represents a repayment installment in a loan schedule.
    /// </summary>
    public class RepaymentDto
    {
        /// <summary>
        /// Unique identifier for the repayment entry.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Scheduled due date for this repayment.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Amount due for this repayment.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Whether the repayment has already been completed.
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// Actual date the repayment was made (if paid).
        /// </summary>
        public DateTime? PaidOn { get; set; }

        /// <summary>
        /// Indicates whether the repayment is overdue and unpaid.
        /// </summary>
        public bool IsOverdue => !IsPaid && DueDate < DateTime.UtcNow;
    }
}
