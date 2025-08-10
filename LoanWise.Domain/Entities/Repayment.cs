using LoanWise.Domain.Common;
using LoanWise.Domain.Events;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a scheduled repayment associated with a loan.
    /// </summary>
    public class Repayment : Entity
    {
        /// <summary>Unique identifier for the repayment entry.</summary>
        public Guid Id { get; private set; }

        /// <summary>Foreign key reference to the parent loan.</summary>
        public Guid LoanId { get; private set; }

        /// <summary>Navigation property to the related loan (optional unless explicitly included).</summary>
        public Loan? Loan { get; private set; }

        /// <summary>Scheduled due date for the repayment.</summary>
        public DateTime DueDate { get; private set; }

        /// <summary>Amount due for this installment (decimal, same currency as the loan).</summary>
        public decimal RepaymentAmount { get; private set; }

        /// <summary>Whether the repayment has been completed.</summary>
        public bool IsPaid { get; private set; }

        /// <summary>Actual payment date if paid.</summary>
        public DateTime? PaidOn { get; private set; }

        /// <summary>Creation timestamp (for ordering/cashflow lists).</summary>
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

        // EF
        private Repayment() { }

        /// <summary>
        /// Creates a new scheduled repayment.
        /// </summary>
        public Repayment(Guid id, Guid loanId, DateTime dueDate, decimal amount)
        {
            if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount), "Repayment amount must be > 0.");

            Id = id;
            LoanId = loanId;
            DueDate = dueDate.Date;
            RepaymentAmount = amount;
            IsPaid = false;
        }

        /// <summary>
        /// Marks the repayment as paid and raises RepaymentPaidEvent.
        /// </summary>
        /// <param name="paymentDate">Date the payment was made (UTC recommended).</param>
        /// <param name="borrowerId">Borrower user id (pass explicitly; do not rely on Loan nav).</param>
        public void MarkAsPaid(DateTime paymentDate, Guid borrowerId)
        {
            if (IsPaid)
                throw new InvalidOperationException("This repayment is already marked as paid.");

            IsPaid = true;
            PaidOn = paymentDate;

            AddDomainEvent(new RepaymentPaidEvent(LoanId, Id, paymentDate));
        }

        /// <summary>
        /// Indicates if this repayment is overdue and unpaid.
        /// </summary>
        public bool IsOverdue(DateTime currentDate) => !IsPaid && currentDate.Date > DueDate;

        /// <summary>
        /// Publishes a due reminder event (explicit borrower id avoids requiring Loan navigation).
        /// </summary>
        public void MarkDue(Guid borrowerId)
        {
            AddDomainEvent(new RepaymentDueEvent(LoanId, borrowerId, Id, DueDate, RepaymentAmount));
        }

        /// <summary>
        /// Publishes an overdue reminder event (explicit borrower id avoids requiring Loan navigation).
        /// </summary>
        public void MarkOverdue(Guid borrowerId)
        {
            AddDomainEvent(new RepaymentOverdueEvent(LoanId, borrowerId, Id, DueDate, RepaymentAmount));
        }
    }
}
