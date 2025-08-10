using LoanWise.Domain.Entities;

namespace LoanWise.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user, CancellationToken cancellationToken);
        Task<User?> GetByIdAsync(Guid id);
        Task<string?> GetEmailByIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
