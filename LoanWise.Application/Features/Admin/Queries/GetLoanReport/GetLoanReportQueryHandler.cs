using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Reports;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Queries.GetLoanReport
{
    public sealed class GetLoanReportQueryHandler
       : IRequestHandler<GetLoanReportQuery, ApiResponse<List<LoanReportDto>>>
    {
        private readonly IApplicationDbContext _db;

        public GetLoanReportQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ApiResponse<List<LoanReportDto>>> Handle(GetLoanReportQuery request, CancellationToken ct)
        {
            var loans = await _db.Loans
                .Include(l => l.Borrower)
                .Include(l => l.Fundings)
                .OrderByDescending(l => l.CreatedAtUtc)
                .Select(l => new LoanReportDto(
                    l.Id,
                    l.Borrower.FullName,
                    l.Amount.Value,
                    l.Purpose.ToString(),
                    l.Status.ToString(),
                    l.RiskLevel.ToString(),
                    l.Fundings.Sum(f => f.Amount.Value),
                    l.CreatedAtUtc,
                    l.ApprovedAtUtc,
                    l.DisbursedAtUtc
                ))
                .ToListAsync(ct);

            return ApiResponse<List<LoanReportDto>>.SuccessResult(loans);
        }
    }
}
