using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Loans.DTOs;
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
        private readonly IUserContext _currentUser;

        public LoanRepository(IApplicationDbContext context, IUserContext currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task AddAsync(Loan loan, CancellationToken cancellationToken)
        {
            await _context.Loans.AddAsync(loan, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Loan loan, CancellationToken cancellationToken)
        {
            var entry = _context.Entry(loan);

            if (entry.State == EntityState.Detached)
            {
                // Only attach/mark modified if truly detached (e.g., coming from a DTO map).
                _context.Loans.Attach(loan);
                entry.State = EntityState.Modified;
            }

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

        /// <summary>
        /// Returns "open" loans:
        /// - Status is Approved or Funded
        /// - NOT Disbursed and NOT Cancelled
        /// - Remaining amount &gt; 0 (Funded &lt; Amount)
        /// Visibility:
        /// - Admin: sees all open loans
        /// - Non-admin (e.g., Lender): restricted to IsVisibleToLenders
        /// </summary>
        public async Task<IReadOnlyList<LoanSummaryDto>> GetOpenLoansAsync(CancellationToken ct)
        {
            var isAdmin = _currentUser.IsInRole("Admin");

            // Base "open" definition
            var query = _context.Loans
                .AsNoTracking()
                .Select(l => new
                {
                    l.Id,
                    l.BorrowerId,
                    l.Amount,
                    Funded = l.Fundings.Select(f => (decimal?)f.Amount).Sum() ?? 0m,
                    l.DurationInMonths,
                    l.Purpose,
                    l.Status,
                    l.IsVisibleToLenders
                })
                .Where(x =>
                    (x.Status == LoanStatus.Approved || x.Status == LoanStatus.Funded) &&
                    x.Status != LoanStatus.Disbursed 
                     &&  x.Funded < x.Amount);

            // Only restrict marketplace visibility for non-admins
            if (!isAdmin)
            {
                query = query.Where(x => x.IsVisibleToLenders);
            }

            return await query
                .OrderBy(x => x.Amount - x.Funded) // smallest remaining first
                .Select(x => new LoanSummaryDto
                {
                    LoanId = x.Id,
                    BorrowerId = x.BorrowerId,
                    Amount = x.Amount,
                    FundedAmount = x.Funded,
                    DurationInMonths = x.DurationInMonths,
                    Purpose = x.Purpose,
                    Status = x.Status
                })
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

        public async Task<Loan?> GetLoanByRepaymentIdWithFundingsAsync(Guid repaymentId, CancellationToken ct)
        {
            return await _context.Loans
                .Include(l => l.Repayments)
                .Include(l => l.Fundings)
                .FirstOrDefaultAsync(l => l.Repayments.Any(r => r.Id == repaymentId), ct);
        }
    }
}
