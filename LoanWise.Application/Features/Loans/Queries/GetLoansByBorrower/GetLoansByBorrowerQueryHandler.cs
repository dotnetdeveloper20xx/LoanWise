using AutoMapper;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Loans.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Queries.GetLoansByBorrower
{
    public class GetLoansByBorrowerQueryHandler : IRequestHandler<GetLoansByBorrowerQuery, ApiResponse<List<BorrowerLoanDto>>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;

        public GetLoansByBorrowerQueryHandler(
            ILoanRepository loanRepository,
            IMapper mapper)
        {
            _loanRepository = loanRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<BorrowerLoanDto>>> Handle(GetLoansByBorrowerQuery request, CancellationToken cancellationToken)
        {
            var loans = await _loanRepository.GetLoansByBorrowerAsync(request.BorrowerId, cancellationToken);

            var result = _mapper.Map<List<BorrowerLoanDto>>(loans);

            return ApiResponse<List<BorrowerLoanDto>>.SuccessResult(result);
        }
    }
}
