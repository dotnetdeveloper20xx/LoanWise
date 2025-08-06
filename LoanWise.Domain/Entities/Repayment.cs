using System;
using LoanWise.Domain.ValueObjects;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a scheduled repayment associated with a loan.
    /// </summary>
    public class Repayment
    {
        /// <summary>
        /// Unique identifier for the repayment entry.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Foreign key reference to the parent loan.
        /// </summary>
        public Guid LoanId { get; private set; }

        /// <summary>
        /// Navigation property to the related loan.
        /// </summary>
        public Loan Loan { get; private set; }

        /// <summary>
        /// Scheduled due date for the repayment.
        /// </summary>
        public DateTime DueDate { get; private set; }

        /// <summary>
        /// Amount due for this installment.
        /// </summary>
        public Money Amount { get; private set; }

        /// <summary>
        /// Whether the repayment has been completed.
        /// </summary>
        public bool IsPaid { get; private set; }

        /// <summary>
        /// Actual payment date if paid.
        /// </summary>
        public DateTime? PaidOn { get; private set; }

        /// <summary>
        /// Required by EF Core.
        /// </summary>
        private Repayment() { }

        /// <summary>
        /// Creates a new scheduled repayment.
        /// </summary>
        /// <param name="id">Unique identifier.</param>
        /// <param name="loanId">Associated loan ID.</param>
        /// <param name="dueDate">Due date for this installment.</param>
        /// <param name="amount">Repayment amount.</param>
        public Repayment(Guid id, Guid loanId, DateTime dueDate, Money amount)
        {
            Id = id;
            LoanId = loanId;
            DueDate = dueDate;
            Amount = amount;
            IsPaid = false;
        }

        /// <summary>
        /// Marks the repayment as paid.
        /// </summary>
        /// <param name="paymentDate">Date the payment was made.</param>
        public void MarkAsPaid(DateTime paymentDate)
        {
            if (IsPaid)
                throw new InvalidOperationException("This repayment is already marked as paid.");

            IsPaid = true;
            PaidOn = paymentDate;
        }

        /// <summary>
        /// Indicates if this repayment is overdue and unpaid.
        /// </summary>
        public bool IsOverdue(DateTime currentDate)
        {
            return !IsPaid && currentDate > DueDate;
        }
    }
}
