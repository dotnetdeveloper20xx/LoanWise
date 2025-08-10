using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Application.Features.Notifications
{
    public sealed class RepaymentPaidNotificationHandler : INotificationHandler<RepaymentPaidEvent>
    {
        private readonly INotificationRepository _repo;
        private readonly ILoanRepository _loans;
        private readonly INotificationService _notify; // <- push (SignalR + Email)

        public RepaymentPaidNotificationHandler(
            INotificationRepository repo,
            ILoanRepository loans,
            INotificationService notify)
        {
            _repo = repo;
            _loans = loans;
            _notify = notify;
        }

        public async Task Handle(RepaymentPaidEvent e, CancellationToken ct)
        {
            var loan = await _loans.GetByIdAsync(e.LoanId, ct);
            if (loan is null) return;

            var repayment = loan.Repayments.FirstOrDefault(r => r.Id == e.RepaymentId);
            if (repayment is null) return;

            var amount = repayment.RepaymentAmount;

            var borrowerTitle = "Repayment recorded";
            var borrowerMsg = $"We recorded your payment of {amount:C} for loan {e.LoanId} on {e.PaidOn:yyyy-MM-dd}.";

            // 1) Push to borrower
            await _notify.NotifyBorrowerAsync(loan.BorrowerId, borrowerTitle, borrowerMsg, ct);

            // 2) Persist borrower inbox
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

            // Optional: notify all lenders who funded this loan (push + persist)
            var lenderTitle = "Repayment received on a funded loan";
            var lenderMsg = $"Loan {e.LoanId} received a repayment of {amount:C} on {e.PaidOn:yyyy-MM-dd}. Your share will be allocated automatically.";

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

            // 3) Persist inbox items (loop is fine; switch to AddRangeAsync if you added it)
            foreach (var n in notifications) { await _repo.AddAsync(n, ct); }
        }
    }
}
