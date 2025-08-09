using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Application.Features.Notifications
{
    public sealed class RepaymentOverdueNotificationHandler : INotificationHandler<RepaymentOverdueEvent>
    {
        private readonly INotificationRepository _repo;
        public RepaymentOverdueNotificationHandler(INotificationRepository repo) => _repo = repo;

        public Task Handle(RepaymentOverdueEvent e, CancellationToken ct) =>
            _repo.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = e.BorrowerId,
                Title = "Repayment overdue",
                Message = $"Repayment {e.RepaymentId} of {e.Amount:C} is OVERDUE (due {e.DueDate:d}).",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            }, ct);
    }
}
