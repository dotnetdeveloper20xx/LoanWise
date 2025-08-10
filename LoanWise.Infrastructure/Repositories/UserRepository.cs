using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace LoanWise.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IApplicationDbContext _context;

        public UserRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }     

        public async Task AddAsync(User user, CancellationToken cancellationToken)
        {
            _context.Users.Add(user);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public Task<string?> GetEmailByIdAsync(Guid userId, CancellationToken ct = default) =>
            _context.Users
               .Where(u => u.Id == userId)
               .Select(u => u.Email)
               .FirstOrDefaultAsync(ct);

    }
}
