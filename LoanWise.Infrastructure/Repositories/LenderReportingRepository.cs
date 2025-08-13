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
            // Defensive paging
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            // FUNDINGS = cash OUT (negative)
            var fundingRows =
                from f in _db.Fundings.AsNoTracking()
                where f.LenderId == lenderId
                select new BaseRow
                {
                    RowId = f.Id,
                    LenderId = f.LenderId,
                    LoanId = f.LoanId,
                    BorrowerId = f.Loan.BorrowerId,
                    BorrowerName = f.Loan.Borrower.FullName,
                    OccurredAtUtc = f.FundedOn,
                    Amount = -f.Amount,     // outflow
                    Kind = "Funding"
                };

            // LENDER REPAYMENTS = cash IN (positive)
            // (Assumes a LenderRepayments table that links LenderId ↔ Repayment allocation.)
            var inflowRows =
                from lr in _db.LenderRepayments.AsNoTracking()
                join l in _db.Loans.AsNoTracking() on lr.LoanId equals l.Id
                join u in _db.Users.AsNoTracking() on l.BorrowerId equals u.Id
                join r in _db.Repayments.AsNoTracking() on lr.RepaymentId equals r.Id
                where lr.LenderId == lenderId
                select new BaseRow
                {
                    RowId = lr.Id,
                    LenderId = lr.LenderId,
                    LoanId = lr.LoanId,
                    BorrowerId = l.BorrowerId,
                    BorrowerName = u.FullName,
                    // If PaidAtUtc is null, fall back to due date — COALESCE is SQL-translatable
                    OccurredAtUtc = r.CreatedAtUtc,
                    Amount = lr.Amount,     // inflow
                    Kind = "Repayment"
                };

            // IMPORTANT: Concat (UNION ALL) BEFORE any client-only projection/formatting.
            IQueryable<BaseRow> all = fundingRows.Concat(inflowRows);

            // Filters (still server-side)
            if (fromUtc.HasValue) all = all.Where(x => x.OccurredAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) all = all.Where(x => x.OccurredAtUtc < toUtc.Value);
            if (loanId.HasValue) all = all.Where(x => x.LoanId == loanId.Value);
            if (borrowerId.HasValue) all = all.Where(x => x.BorrowerId == borrowerId.Value);

            // Total BEFORE paging (DB-side)
            var total = await all.CountAsync(ct);

            // Order + page in SQL, then materialize a simple shape
            var pageRows = await all
                .OrderByDescending(x => x.OccurredAtUtc)
                .ThenByDescending(x => x.RowId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.RowId,
                    x.LenderId,
                    x.LoanId,
                    x.BorrowerName,
                    x.OccurredAtUtc,
                    x.Amount,
                    x.Kind
                })
                .ToListAsync(ct);

            // Final projection to DTO IN MEMORY (safe place for formatting / ToString / string concat)
            var items = pageRows.Select(x => new LenderTransactionDto
            {
                Id = x.RowId,
                LenderId = x.LenderId,
                LoanId = x.LoanId,
                LoanRef = x.LoanId.ToString(), // formatting now in-memory
                BorrowerName = x.BorrowerName,
                OccurredAtUtc = x.OccurredAtUtc,
                Type = x.Kind,
                Amount = x.Amount,
                Description = x.Kind == "Funding"
                                ? $"Funding to loan {x.LoanId}"
                                : $"Repayment allocation for loan {x.LoanId}"
            });

            return (total, items);
        }

        // Minimal server-translatable shape used BEFORE set operations.
        private sealed class BaseRow
        {
            public Guid RowId { get; set; }
            public Guid LenderId { get; set; }
            public Guid LoanId { get; set; }
            public Guid BorrowerId { get; set; }
            public string BorrowerName { get; set; } = default!;
            public DateTime OccurredAtUtc { get; set; }
            public decimal Amount { get; set; }
            public string Kind { get; set; } = default!;
        }
    }
}
