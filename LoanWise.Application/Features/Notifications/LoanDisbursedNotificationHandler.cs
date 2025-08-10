using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Application.Features.Notifications
{
    public sealed class LoanDisbursedNotificationHandler : INotificationHandler<LoanDisbursedEvent>
    {
        private readonly INotificationRepository _repo;
        private readonly ILoanRepository _loans;

        public LoanDisbursedNotificationHandler(INotificationRepository repo, ILoanRepository loans)
        {
            _repo = repo;
            _loans = loans;
        }

        public async Task Handle(LoanDisbursedEvent e, CancellationToken ct)
        {
            var loan = await _loans.GetByIdAsync(e.LoanId, ct);
            if (loan is null) return;

            var notifications = new List<Notification>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = loan.BorrowerId, // recipient
                Title = "Loan disbursed",
                Message = $"Your loan {e.LoanId} has been disbursed on {e.DisbursedOn:yyyy-MM-dd}.",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            }
        };

            // Optional: notify lenders too
            foreach (var lenderId in loan.Fundings.Select(f => f.LenderId).Distinct())
            {
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = lenderId, // recipients (lenders)
                    Title = "A funded loan was disbursed",
                    Message = $"Loan {e.LoanId} you helped fund has been disbursed.",
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            foreach (var notification in notifications)
            {
                await _repo.AddAsync(notification, ct);
            }
        }
    }

}
