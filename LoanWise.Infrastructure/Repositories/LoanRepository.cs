using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LoanWise.Infrastructure.Repositories
{
    /// <summary>
    /// Provides EF Core-based data access for Loan entities.
    /// </summary>
    public class LoanRepository : ILoanRepository
    {
        private readonly IApplicationDbContext _context;

        public LoanRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Loan loan, CancellationToken cancellationToken)
        {
            await _context.Loans.AddAsync(loan, cancellationToken);
        }

        public async Task<Loan?> GetByIdAsync(Guid loanId, CancellationToken cancellationToken)
        {
            return await _context.Loans
                .Include(l => l.Fundings)
                .Include(l => l.Repayments)
                .Include(l => l.Borrower)
                .FirstOrDefaultAsync(l => l.Id == loanId, cancellationToken);
        }

        public async Task<IEnumerable<Loan>> GetOpenLoansAsync(CancellationToken cancellationToken)
        {
            return await _context.Loans
                .Where(l =>
                    l.Status == LoanStatus.Approved &&
                    l.Fundings.Sum(f => f.Amount.Value) < l.Amount.Value)
                .Include(l => l.Borrower)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Loan>> GetLoansByBorrowerAsync(Guid borrowerId, CancellationToken cancellationToken)
        {
            return await _context.Loans
                .Where(l => l.BorrowerId == borrowerId)
                .Include(l => l.Repayments)
                .Include(l => l.Fundings)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
