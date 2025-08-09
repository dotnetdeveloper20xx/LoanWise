using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;

namespace LoanWise.Infrastructure.Repositories
{
    public sealed class NotificationRepository : INotificationRepository
    {
        private readonly IApplicationDbContext _db;
        public NotificationRepository(IApplicationDbContext db) => _db = db;

        public Task AddAsync(Notification notification, CancellationToken ct = default)
            => _db.Notifications.AddAsync(notification, ct).AsTask();
    }

}


