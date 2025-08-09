using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Application.Features.Notifications
{
    public sealed class LoanRejectedNotificationHandler : INotificationHandler<LoanRejectedEvent>
    {
        private readonly INotificationRepository _repo;
        public LoanRejectedNotificationHandler(INotificationRepository repo) => _repo = repo;

        public Task Handle(LoanRejectedEvent e, CancellationToken ct) =>
            _repo.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = e.BorrowerId,
                Title = "Loan rejected",
                Message = $"Your loan {e.LoanId} was rejected. Reason: {e.Reason}",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            }, ct);
    }
}
