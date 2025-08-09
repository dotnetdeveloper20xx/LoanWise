using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Application.Features.Notifications
{
    public sealed class LoanDisbursedNotificationHandler : INotificationHandler<LoanDisbursedEvent>
    {
        private readonly INotificationRepository _repo;
        public LoanDisbursedNotificationHandler(INotificationRepository repo) => _repo = repo;

        public Task Handle(LoanDisbursedEvent e, CancellationToken ct) =>
            _repo.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = e.BorrowerId,
                Title = "Loan disbursed",
                Message = $"Your loan {e.LoanId} has been disbursed.",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            }, ct);
    }
}
