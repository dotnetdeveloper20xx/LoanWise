using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Loans.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Queries.GetLoansByBorrower
{
    public class GetLoansByBorrowerQueryHandler : IRequestHandler<GetLoansByBorrowerQuery, ApiResponse<List<BorrowerLoanDto>>>
    {
        private readonly ILoanRepository _loanRepository;

        public GetLoansByBorrowerQueryHandler(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<ApiResponse<List<BorrowerLoanDto>>> Handle(GetLoansByBorrowerQuery request, CancellationToken cancellationToken)
        {
            var loans = await _loanRepository.GetLoansByBorrowerAsync(request.BorrowerId, cancellationToken);

            var result = loans.Select(loan => new BorrowerLoanDto
            {
                LoanId = loan.Id,
                Amount = loan.Amount.Value,
                DurationInMonths = loan.DurationInMonths,
                Purpose = loan.Purpose,
                Status = loan.Status,
                RiskLevel = loan.RiskLevel,
                FundedAmount = loan.Fundings.Sum(f => f.Amount.Value)
            }).ToList();

            return ApiResponse<List<BorrowerLoanDto>>.SuccessResult(result);
        }
    }
}
