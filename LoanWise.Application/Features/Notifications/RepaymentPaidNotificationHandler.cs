using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;

namespace LoanWise.Application.Features.Notifications
{
    using MediatR;

    public sealed class RepaymentPaidNotificationHandler : INotificationHandler<RepaymentPaidEvent>
    {
        private readonly INotificationRepository _repo;
        private readonly ILoanRepository _loans;

        public RepaymentPaidNotificationHandler(INotificationRepository repo, ILoanRepository loans)
        {
            _repo = repo;
            _loans = loans;
        }

        public async Task Handle(RepaymentPaidEvent e, CancellationToken ct)
        {
            // Load loan and find the specific repayment
            var loan = await _loans.GetByIdAsync(e.LoanId, ct);
            if (loan is null) return;

            var repayment = loan.Repayments.FirstOrDefault(r => r.Id == e.RepaymentId);
            if (repayment is null) return;

            var amount = repayment.RepaymentAmount;

            var notifications = new List<Notification>
        {
            // Recipient: Borrower
            new()
            {
                Id = Guid.NewGuid(),
                UserId = loan.BorrowerId,
                Title = "Repayment recorded",
                Message = $"We recorded your payment of {amount:C} for loan {e.LoanId} on {e.PaidOn:yyyy-MM-dd}.",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            }
        };

            // Optional: notify all lenders who funded this loan
            foreach (var lenderId in loan.Fundings.Select(f => f.LenderId).Distinct())
            {
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = lenderId,
                    Title = "Repayment received on a funded loan",
                    Message = $"Loan {e.LoanId} received a repayment of {amount:C} on {e.PaidOn:yyyy-MM-dd}. Your share will be allocated automatically.",
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            // Persist
            foreach (var n in notifications) { await _repo.AddAsync(n, ct); }
        }
    }

}
