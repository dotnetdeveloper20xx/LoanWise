using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanWise.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of IFundingRepository.
    /// </summary>
    public class FundingRepository : IFundingRepository
    {
        private readonly IApplicationDbContext _context;

        public FundingRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Funding funding, CancellationToken cancellationToken = default)
        {
            await _context.Fundings.AddAsync(funding, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Funding>> GetByLoanIdAsync(Guid loanId, CancellationToken cancellationToken = default)
        {
            return await _context.Fundings
                .Where(f => f.LoanId == loanId)
                .ToListAsync(cancellationToken);
        }
    }
}
