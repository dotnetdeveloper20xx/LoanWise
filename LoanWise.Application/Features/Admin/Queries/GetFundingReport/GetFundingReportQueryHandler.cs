using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Reports;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Queries.GetFundingReport
{
    public sealed class GetFundingReportQueryHandler
          : IRequestHandler<GetFundingReportQuery, ApiResponse<List<FundingReportDto>>>
    {
        private readonly IApplicationDbContext _db;

        public GetFundingReportQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ApiResponse<List<FundingReportDto>>> Handle(GetFundingReportQuery request, CancellationToken ct)
        {
            var fundings = await _db.Fundings
                .Include(f => f.Loan)
                    .ThenInclude(l => l.Borrower)
                .Include(f => f.Lender)
                .OrderByDescending(f => f.FundedOn)
                .Select(f => new FundingReportDto(
                    f.Id,
                    f.LoanId,
                    f.Loan.Borrower.FullName,
                    f.LenderId,
                    f.Lender.FullName,
                    f.Amount.Value, // or direct decimal if not a Money object
                    f.FundedOn,
                    f.Loan.Status.ToString()
                ))
                .ToListAsync(ct);

            return ApiResponse<List<FundingReportDto>>.SuccessResult(fundings);
        }
    }
}
