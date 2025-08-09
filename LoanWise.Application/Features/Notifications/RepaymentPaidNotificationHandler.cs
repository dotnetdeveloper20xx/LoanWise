using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Application.Features.Notifications
{
    public sealed class RepaymentPaidNotificationHandler : INotificationHandler<RepaymentPaidEvent>
    {
        private readonly INotificationRepository _repo;
        public RepaymentPaidNotificationHandler(INotificationRepository repo) => _repo = repo;

        public Task Handle(RepaymentPaidEvent e, CancellationToken ct) =>
            _repo.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = e.BorrowerId,
                Title = "Repayment received",
                Message = $"Payment of {e.Amount:C} received for loan {e.LoanId}.",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            }, ct);
    }
}
