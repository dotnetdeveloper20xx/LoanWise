using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Application.Features.Notifications
{
    public sealed class LoanApprovedNotificationHandler : INotificationHandler<LoanApprovedEvent>
    {
        private readonly INotificationRepository _repo;
        public LoanApprovedNotificationHandler(INotificationRepository repo) => _repo = repo;

        public async Task Handle(LoanApprovedEvent @event, CancellationToken ct)
        {
            await _repo.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = @event.BorrowerId,
                Title = "Loan approved",
                Message = $"Your loan {@event.LoanId} has been approved and is open for funding.",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            }, ct);
        }
    }
}
