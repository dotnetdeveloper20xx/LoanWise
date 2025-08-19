using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Loans.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Queries.GetOpenLoans
{
    public class GetOpenLoansQueryHandler
        : IRequestHandler<GetOpenLoansQuery, ApiResponse<List<LoanSummaryDto>>>
    {
        private readonly ILoanRepository _loanRepository;

        public GetOpenLoansQueryHandler(ILoanRepository loanRepository)
            => _loanRepository = loanRepository;

        public async Task<ApiResponse<List<LoanSummaryDto>>> Handle(
            GetOpenLoansQuery request,
            CancellationToken ct)
        {
            var items = await _loanRepository.GetOpenLoansAsync(ct);

            return items.Count == 0
                ? ApiResponse<List<LoanSummaryDto>>
                    .SuccessResult(new List<LoanSummaryDto>(), "No open loans right now.")
                : ApiResponse<List<LoanSummaryDto>>
                    .SuccessResult(items.ToList());
        }
    }
}
