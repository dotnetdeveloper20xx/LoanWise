using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Dashboard;
using LoanWise.Application.Features.Loans.Commands.ApplyLoan;
using LoanWise.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Dashboard.Queries.GetBorrowerDashboard
{
    public class GetBorrowerDashboardQueryHandler : IRequestHandler<GetBorrowerDashboardQuery, ApiResponse<BorrowerDashboardDto>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IUserContext _userContext;
        private readonly ILogger<ApplyLoanCommandHandler> _logger;

        public GetBorrowerDashboardQueryHandler(
            ILoanRepository loanRepository,
            ILogger<ApplyLoanCommandHandler> logger,
            IUserContext userContext)
        {
            _loanRepository = loanRepository;
            _userContext = userContext;          
            _logger = logger;
        }

        public async Task<ApiResponse<BorrowerDashboardDto>> Handle(GetBorrowerDashboardQuery request, CancellationToken cancellationToken)
        {
            if (!_userContext.UserId.HasValue)
                return ApiResponse<BorrowerDashboardDto>.FailureResult("Unauthorized: missing user ID");

            var borrowerId = _userContext.UserId.Value;

            var loans = await _loanRepository.GetLoansByBorrowerAsync(borrowerId, cancellationToken);

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
