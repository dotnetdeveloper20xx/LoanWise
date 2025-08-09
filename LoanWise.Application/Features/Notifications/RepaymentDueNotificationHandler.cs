using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Application.Features.Notifications
{
    public sealed class RepaymentDueNotificationHandler : INotificationHandler<RepaymentDueEvent>
    {
        private readonly INotificationRepository _repo;
        public RepaymentDueNotificationHandler(INotificationRepository repo) => _repo = repo;

        public Task Handle(RepaymentDueEvent e, CancellationToken ct) =>
            _repo.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = e.BorrowerId,
                Title = "Repayment due",
                Message = $"Repayment {e.RepaymentId} of {e.Amount:C} is due on {e.DueDate:d}.",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            }, ct);
    }
}
