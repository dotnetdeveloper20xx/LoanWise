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
        private readonly IUserContext _userContext;

        public GetLoansByBorrowerQueryHandler(
            ILoanRepository loanRepository,
            IMapper mapper,
            IUserContext userContext)
        {
            _loanRepository = loanRepository;
            _mapper = mapper;
            _userContext = userContext;
        }

        public async Task<ApiResponse<List<BorrowerLoanDto>>> Handle(GetLoansByBorrowerQuery request, CancellationToken cancellationToken)
        {
            if (!_userContext.UserId.HasValue)
                return ApiResponse<List<BorrowerLoanDto>>.FailureResult("Unauthorized: missing user ID");

            var borrowerId = _userContext.UserId.Value;

            var loans = await _loanRepository.GetLoansByBorrowerAsync(borrowerId, cancellationToken);
            var result = _mapper.Map<List<BorrowerLoanDto>>(loans);

            return ApiResponse<List<BorrowerLoanDto>>.SuccessResult(result);
        }
    }
}
