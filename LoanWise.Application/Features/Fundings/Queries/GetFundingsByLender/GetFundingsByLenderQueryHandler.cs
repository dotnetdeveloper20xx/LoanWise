using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Fundings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Fundings.Queries.GetFundingsByLender
{
    public class GetFundingsByLenderQueryHandler : IRequestHandler<GetFundingsByLenderQuery, ApiResponse<List<LenderFundingDto>>>
    {
        private readonly ILoanRepository _loanRepository;

        public GetFundingsByLenderQueryHandler(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<ApiResponse<List<LenderFundingDto>>> Handle(GetFundingsByLenderQuery request, CancellationToken cancellationToken)
        {
            var loans = await _loanRepository.GetAllIncludingFundingsAsync(cancellationToken);

            var result = loans
                .Where(loan => loan.Fundings.Any(f => f.LenderId == request.LenderId))
                .Select(loan => new LenderFundingDto
                {
                    LoanId = loan.Id,
                    LoanAmount = loan.Amount.Value,
                    TotalFunded = loan.Fundings.Sum(f => f.Amount.Value),
                    AmountFundedByYou = loan.Fundings
                        .Where(f => f.LenderId == request.LenderId)
                        .Sum(f => f.Amount.Value),
                    Purpose = loan.Purpose,
                    Status = loan.Status
                })
                .ToList();

            return ApiResponse<List<LenderFundingDto>>.SuccessResult(result);
        }
    }
}
