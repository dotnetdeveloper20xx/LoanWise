using AutoMapper;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Loans.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Queries.GetOpenLoans
{
    public class GetOpenLoansQueryHandler : IRequestHandler<GetOpenLoansQuery, ApiResponse<List<LoanSummaryDto>>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;

        public GetOpenLoansQueryHandler(ILoanRepository loanRepository, IMapper mapper)
        {
            _loanRepository = loanRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<LoanSummaryDto>>> Handle(GetOpenLoansQuery request, CancellationToken ct)
        {
            var openLoans = await _loanRepository.GetOpenLoansAsync(ct);
            var result = _mapper.Map<List<LoanSummaryDto>>(openLoans);   
            return ApiResponse<List<LoanSummaryDto>>.SuccessResult(result);
        }
    }
}
