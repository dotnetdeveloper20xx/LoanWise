using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Infrastructure.Notifications.Handlers
{
    public sealed class LoanFundedEventHandler : INotificationHandler<LoanFundedEvent>
    {
        private readonly INotificationRepository _repo;
        private readonly ILoanRepository _loans;

        public LoanFundedEventHandler(INotificationRepository repo, ILoanRepository loans)
        {
            _repo = repo;
            _loans = loans;
        }

        public async Task Handle(LoanFundedEvent e, CancellationToken ct)
        {
            // Load the loan to get the borrower (event no longer carries BorrowerId)
            var loan = await _loans.GetByIdAsync(e.LoanId, ct);
            if (loan is null) return;

            var notifications = new List<Notification>
            {
                // Notify borrower about the new funding and whether it's now fully funded
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = loan.BorrowerId, // recipient
                    Title = e.IsFullyFunded ? "Loan fully funded" : "New funding received",
                    Message = e.IsFullyFunded
                        ? $"Your loan {e.LoanId} just received £{e.Amount:N2} and is now fully funded."
                        : $"Your loan {e.LoanId} just received £{e.Amount:N2}.",
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                },

                // Optional: confirmation to the contributing lender
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = e.LenderId, // recipient
                    Title = "Funding confirmed",
                    Message = $"You funded £{e.Amount:N2} to loan {e.LoanId}.",
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                }
            };

            // Persist notifications

            foreach (var n in notifications) await _repo.AddAsync(n, ct);
        }
    }
}
