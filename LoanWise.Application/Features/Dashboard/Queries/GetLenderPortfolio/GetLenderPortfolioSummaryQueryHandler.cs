using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Dashboard.Queries.GetLenderPortfolio
{
    public class GetLenderPortfolioSummaryQueryHandler : IRequestHandler<GetLenderPortfolioSummaryQuery, ApiResponse<LenderPortfolioDto>>
    {
        private readonly ILoanRepository _loanRepository;

        public GetLenderPortfolioSummaryQueryHandler(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<ApiResponse<LenderPortfolioDto>> Handle(GetLenderPortfolioSummaryQuery request, CancellationToken cancellationToken)
        {
            var loans = await _loanRepository.GetAllIncludingFundingsAsync(cancellationToken);

            var relevantLoans = loans
                .Where(loan => loan.Fundings.Any(f => f.LenderId == request.LenderId))
                .ToList();

            var totalFunded = relevantLoans
                .SelectMany(l => l.Fundings)
                .Where(f => f.LenderId == request.LenderId)
                .Sum(f => f.Amount.Value);

            var numberOfLoans = relevantLoans.Count;

            var dto = new LenderPortfolioDto
            {
                TotalFunded = totalFunded,
                NumberOfLoansFunded = numberOfLoans,
                TotalReceived = 0, // to be implemented in future
                OutstandingBalance = 0 // to be implemented
            };

            return ApiResponse<LenderPortfolioDto>.SuccessResult(dto);
        }
    }
}
