using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Lenders;
using Microsoft.EntityFrameworkCore;

namespace LoanWise.Infrastructure.Repositories
{
    internal sealed class LenderReportingRepository : ILenderReportingRepository
    {
        private readonly IApplicationDbContext _db;
        public LenderReportingRepository(IApplicationDbContext db) => _db = db;

        public async Task<(int total, IEnumerable<LenderTransactionDto> items)> GetLenderTransactionsAsync(
            Guid lenderId, DateTime? fromUtc, DateTime? toUtc, Guid? loanId, Guid? borrowerId,
            int page, int pageSize, CancellationToken ct)
        {
            // FUNDINGS = cash out (negative)
            var fundings = from f in _db.Fundings.AsNoTracking()
                           join l in _db.Loans.AsNoTracking() on f.LoanId equals l.Id
                           join u in _db.Users.AsNoTracking() on l.BorrowerId equals u.Id
                           where f.LenderId == lenderId
                           select new LenderTransactionDto
                           {
                               Id = f.Id,
                               LenderId = f.LenderId,
                               LoanId = f.LoanId,
                               LoanRef = l.Id.ToString(), // or l.Reference if you have one
                               BorrowerName = u.FullName, // adjust to your user field
                               OccurredAtUtc = f.FundedOn,
                               Type = "Funding",
                               Amount = -f.Amount.Value,  // negative outflow
                               Description = $"Funding to loan {l.Id}"
                           };

            // LENDER REPAYMENTS = cash in (positive)
            var inflows = from lr in _db.LenderRepayments.AsNoTracking()
                          join l in _db.Loans.AsNoTracking() on lr.LoanId equals l.Id
                          join u in _db.Users.AsNoTracking() on l.BorrowerId equals u.Id
                          join r in _db.Repayments.AsNoTracking() on lr.RepaymentId equals r.Id
                          where lr.LenderId == lenderId
                          select new LenderTransactionDto
                          {
                              Id = lr.Id,
                              LenderId = lr.LenderId,
                              LoanId = lr.LoanId,
                              LoanRef = l.Id.ToString(),
                              BorrowerName = u.FullName,
                              OccurredAtUtc = r.CreatedAtUtc,
                              Type = "Repayment",
                              Amount = lr.Amount,         // positive inflow
                              Description = $"Repayment allocation for loan {l.Id}"
                          };

            // UNION (Concat in EF)
            var all = fundings.Concat(inflows);

            // Filters
            if (fromUtc.HasValue) all = all.Where(t => t.OccurredAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) all = all.Where(t => t.OccurredAtUtc <= toUtc.Value);
            if (loanId.HasValue) all = all.Where(t => t.LoanId == loanId.Value);
            if (borrowerId.HasValue)
            {
                // join to loans if you prefer by borrower id
                all = from t in all
                      join l in _db.Loans.AsNoTracking() on t.LoanId equals l.Id
                      where l.BorrowerId == borrowerId.Value
                      select t;
            }

            // Total (pre-paging)
            var total = await all.CountAsync(ct);

            // Page
            var items = await all
                .OrderByDescending(t => t.OccurredAtUtc)
                .ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (total, items);
        }
    }
}
