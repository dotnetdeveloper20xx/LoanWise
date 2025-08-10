using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Application.Features.Notifications
{
    public sealed class RepaymentOverdueNotificationHandler : INotificationHandler<RepaymentOverdueEvent>
    {
        private readonly INotificationRepository _repo;
        private readonly ILoanRepository _loans;
        private readonly INotificationService _notify;

        public RepaymentOverdueNotificationHandler(
            INotificationRepository repo,
            ILoanRepository loans,
            INotificationService notify)
        {
            _repo = repo;
            _loans = loans;
            _notify = notify;
        }

        public async Task Handle(RepaymentOverdueEvent e, CancellationToken ct)
        {
            var loan = await _loans.GetByIdAsync(e.LoanId, ct);
            if (loan is null) return;

            var repayment = loan.Repayments.FirstOrDefault(r => r.Id == e.RepaymentId);
            if (repayment is null) return;

            var amount = repayment.RepaymentAmount;
            var due = e.DueDate;

            // Borrower (push + persist)
            var borrowerTitle = "Repayment overdue";
            var borrowerMsg = $"Your repayment of {amount:C} for loan {e.LoanId} was due on {due:yyyy-MM-dd}. Please pay as soon as possible.";

            await _notify.NotifyBorrowerAsync(loan.BorrowerId, borrowerTitle, borrowerMsg, ct);

            var notifications = new List<Notification>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = loan.BorrowerId,
                    Title = borrowerTitle,
                    Message = borrowerMsg,
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                }
            };

            // Lenders (push + persist)
            var lenderTitle = "Overdue on a funded loan";
            var lenderMsg = $"Loan {e.LoanId} has an overdue repayment of {amount:C} (due {due:yyyy-MM-dd}). We will keep you posted.";

            foreach (var lenderId in loan.Fundings.Select(f => f.LenderId).Distinct())
            {
                await _notify.NotifyLenderAsync(lenderId, lenderTitle, lenderMsg, ct);
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = lenderId,
                    Title = lenderTitle,
                    Message = lenderMsg,
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            foreach (var n in notifications)
                await _repo.AddAsync(n, ct);
            
        }
    }
}
