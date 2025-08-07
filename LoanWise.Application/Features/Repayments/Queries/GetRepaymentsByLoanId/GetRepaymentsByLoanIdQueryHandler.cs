using AutoMapper;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Repayments;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Repayments.Queries.GetRepaymentsByLoanId
{
    public class GetRepaymentsByLoanIdQueryHandler : IRequestHandler<GetRepaymentsByLoanIdQuery, ApiResponse<List<RepaymentDto>>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;

        public GetRepaymentsByLoanIdQueryHandler(ILoanRepository loanRepository, IMapper mapper)
        {
            _loanRepository = loanRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<RepaymentDto>>> Handle(GetRepaymentsByLoanIdQuery request, CancellationToken cancellationToken)
        {
            var loan = await _loanRepository.GetByIdAsync(request.LoanId, cancellationToken);

            if (loan == null)
                return ApiResponse<List<RepaymentDto>>.FailureResult("Loan not found.");

            var result = _mapper.Map<List<RepaymentDto>>(loan.Repayments.ToList());

            return ApiResponse<List<RepaymentDto>>.SuccessResult(result);
        }
    }
}
