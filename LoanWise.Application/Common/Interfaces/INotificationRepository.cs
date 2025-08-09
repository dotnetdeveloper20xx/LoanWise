using LoanWise.Domain.Entities;

namespace LoanWise.Application.Common.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    }
}
