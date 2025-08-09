using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Application.Features.Notifications
{
    public sealed class FundingAddedNotificationHandler : INotificationHandler<FundingAddedEvent>
    {
        private readonly INotificationRepository _repo;
        public FundingAddedNotificationHandler(INotificationRepository repo) => _repo = repo;

        public Task Handle(FundingAddedEvent e, CancellationToken ct) =>
            _repo.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = e.LenderId,
                Title = "Funding successful",
                Message = $"You funded {e.Amount:C} to loan {e.LoanId}.",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            }, ct);
    }
}
