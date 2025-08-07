using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Loans.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Queries.GetOpenLoans
{
    public class GetOpenLoansQueryHandler : IRequestHandler<GetOpenLoansQuery, ApiResponse<List<LoanSummaryDto>>>
    {
        private readonly ILoanRepository _loanRepository;

        public GetOpenLoansQueryHandler(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<ApiResponse<List<LoanSummaryDto>>> Handle(GetOpenLoansQuery request, CancellationToken cancellationToken)
        {
            var openLoans = await _loanRepository.GetOpenLoansAsync(cancellationToken);

            var result = openLoans.Select(loan => new LoanSummaryDto
            {
                LoanId = loan.Id,
                BorrowerId = loan.BorrowerId,
                Amount = loan.Amount.Value,
                FundedAmount = loan.Fundings.Sum(f => f.Amount.Value),
                DurationInMonths = loan.DurationInMonths,
                Purpose = loan.Purpose,
                Status = loan.Status
            }).ToList();

            return ApiResponse<List<LoanSummaryDto>>.SuccessResult(result);
        }
    }
}
