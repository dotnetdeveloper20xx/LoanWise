using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Dashboard;
using LoanWise.Domain.Enums;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Dashboard.Queries.GetBorrowerDashboard
{
    public class GetBorrowerDashboardQueryHandler : IRequestHandler<GetBorrowerDashboardQuery, ApiResponse<BorrowerDashboardDto>>
    {
        private readonly ILoanRepository _loanRepository;

        public GetBorrowerDashboardQueryHandler(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<ApiResponse<BorrowerDashboardDto>> Handle(GetBorrowerDashboardQuery request, CancellationToken cancellationToken)
        {
            var loans = await _loanRepository.GetLoansByBorrowerAsync(request.BorrowerId, cancellationToken);

            var totalLoans = loans.Count();
            var fundedLoans = loans.Count(l => l.Status == LoanStatus.Funded || l.Status == LoanStatus.Disbursed || l.Status == LoanStatus.Completed);
            var disbursedLoans = loans.Count(l => l.Status == LoanStatus.Disbursed || l.Status == LoanStatus.Completed);

            var allUnpaidRepayments = loans
                .SelectMany(l => l.Repayments)
                .Where(r => !r.IsPaid)
                .OrderBy(r => r.DueDate)
                .ToList();

            var nextRepayment = allUnpaidRepayments.FirstOrDefault();

            var totalOutstanding = allUnpaidRepayments.Sum(r => r.Amount.Value);

            var dto = new BorrowerDashboardDto
            {
                TotalLoans = totalLoans,
                FundedLoans = fundedLoans,
                DisbursedLoans = disbursedLoans,
                TotalOutstanding = totalOutstanding,
                UpcomingRepaymentId = nextRepayment?.Id,
                NextRepaymentDueDate = nextRepayment?.DueDate,
                NextRepaymentAmount = nextRepayment?.Amount.Value
            };

            return ApiResponse<BorrowerDashboardDto>.SuccessResult(dto);
        }
    }
}
