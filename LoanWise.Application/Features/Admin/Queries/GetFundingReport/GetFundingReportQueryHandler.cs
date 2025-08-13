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
                .AsNoTracking()
                .OrderByDescending(f => f.FundedOn)
                .Select(f => new FundingReportDto(
                    f.Id,
                    f.LoanId,
                    f.Loan != null && f.Loan.Borrower != null ? f.Loan.Borrower.FullName : "(Unknown Borrower)",
                    f.LenderId,
                    f.Lender != null ? f.Lender.FullName : "(Unknown Lender)",
                    f.Amount,
                    f.FundedOn,
                    f.Loan != null ? f.Loan.Status.ToString() : "Unknown"
                ))
                .ToListAsync(ct);


            return ApiResponse<List<FundingReportDto>>.SuccessResult(fundings);
        }
    }
}
