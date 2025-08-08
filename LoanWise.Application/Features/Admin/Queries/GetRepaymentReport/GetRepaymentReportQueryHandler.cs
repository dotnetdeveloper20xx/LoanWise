using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Reports;
using LoanWise.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;
using System.Linq;

namespace LoanWise.Application.Features.Admin.Queries.GetRepaymentReport
{
    public sealed class GetRepaymentReportQueryHandler
          : IRequestHandler<GetRepaymentReportQuery, ApiResponse<List<RepaymentReportDto>>>
    {
        private readonly IApplicationDbContext _db;

        public GetRepaymentReportQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ApiResponse<List<RepaymentReportDto>>> Handle(GetRepaymentReportQuery request, CancellationToken ct)
        {
            var repayments = await _db.Repayments
                .Include(r => r.Loan)
                    .ThenInclude(l => l.Borrower)
                .OrderByDescending(r => r.DueDate)
                .Select(r => new RepaymentReportDto(
                    r.Id,
                    r.LoanId,
                    r.Loan.Borrower.FullName,
                    r.DueDate,
                    r.IsPaid,
                    r.PaidOn,
                   r.RepaymentAmount,
                    r.Loan.Status.ToString()
                ))
                .ToListAsync(ct);

            return ApiResponse<List<RepaymentReportDto>>.SuccessResult(repayments);
        }
    }
}
