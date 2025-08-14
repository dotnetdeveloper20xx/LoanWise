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
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Loan loan, CancellationToken cancellationToken)
        {
            _context.Loans.Update(loan);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Loan?> GetByIdAsync(Guid loanId, CancellationToken cancellationToken)
        {
            return await _context.Loans
                .Include(l => l.Borrower)
                .Include(l => l.Fundings)
                .Include(l => l.Repayments)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == loanId, cancellationToken);
        }


        public async Task<IReadOnlyList<Loan>> GetOpenLoansAsync(CancellationToken ct)
        {
            return await _context.Loans
                .AsNoTracking()
                .Include(l => l.Fundings) // so mapping can use l.Fundings.Sum(...)
                .Where(l => l.Status == LoanStatus.Approved)
                .Where(l => (l.Fundings.Select(f => (decimal?)f.Amount).Sum() ?? 0m) < l.Amount) // still has remaining
                .OrderBy(l => l.Amount - (l.Fundings.Select(f => (decimal?)f.Amount).Sum() ?? 0m)) // smallest remaining first
                .ToListAsync(ct);
        }


        public async Task<IReadOnlyList<Loan>> GetLoansByBorrowerAsync(Guid borrowerId, CancellationToken cancellationToken)
        {
            return await _context.Loans
                .Where(l => l.BorrowerId == borrowerId)
                .Include(l => l.Repayments)
                .Include(l => l.Fundings)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Loan>> GetByStatusAsync(LoanStatus status, CancellationToken cancellationToken)
        {
            return await _context.Loans
                .Where(l => l.Status == status)
                .Include(l => l.Borrower)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Loan>> GetAllIncludingFundingsAsync(CancellationToken cancellationToken)
        {
            return await _context.Loans
                .Include(l => l.Fundings)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Loan>> GetLoansWithRepaymentsAsync(CancellationToken cancellationToken)
        {
            return await _context.Loans
                .Include(l => l.Repayments)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Loan>> GetAllIncludingRepaymentsAsync(CancellationToken cancellationToken)
        {
            return await _context.Loans
                .Include(l => l.Repayments)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
      
        public async Task<Loan?> GetByIdWithRepaymentsAsync(Guid id, CancellationToken ct)
        {
            return await _context.Loans
                .Include(l => l.Repayments)
                .FirstOrDefaultAsync(l => l.Id == id, ct);
        }

    }
}
