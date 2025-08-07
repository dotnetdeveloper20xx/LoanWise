using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Dashboard;
using LoanWise.Domain.Enums;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Dashboard.Queries.GetAdminLoanStats
{
    public class GetAdminLoanStatsQueryHandler : IRequestHandler<GetAdminLoanStatsQuery, ApiResponse<AdminLoanStatsDto>>
    {
        private readonly ILoanRepository _loanRepository;

        public GetAdminLoanStatsQueryHandler(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<ApiResponse<AdminLoanStatsDto>> Handle(GetAdminLoanStatsQuery request, CancellationToken cancellationToken)
        {
            var loans = await _loanRepository.GetAllIncludingRepaymentsAsync(cancellationToken);

            var stats = new AdminLoanStatsDto
            {
                TotalLoans = loans.Count(),
                ApprovedCount = loans.Count(l => l.Status == LoanStatus.Approved),
                FundedCount = loans.Count(l => l.Status == LoanStatus.Funded),
                DisbursedCount = loans.Count(l => l.Status == LoanStatus.Disbursed),
                CompletedCount = loans.Count(l => l.Status == LoanStatus.Completed),
                OverdueRepaymentCount = loans
                    .SelectMany(l => l.Repayments)
                    .Count(r => !r.IsPaid && r.DueDate < DateTime.UtcNow)
            };

            return ApiResponse<AdminLoanStatsDto>.SuccessResult(stats);
        }
    }
}
