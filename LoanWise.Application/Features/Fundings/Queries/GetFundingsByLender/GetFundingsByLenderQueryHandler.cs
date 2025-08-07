using AutoMapper;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Fundings.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Fundings.Queries.GetFundingsByLender
{
    public class GetFundingsByLenderQueryHandler : IRequestHandler<GetFundingsByLenderQuery, ApiResponse<List<LenderFundingDto>>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;

        public GetFundingsByLenderQueryHandler(ILoanRepository loanRepository, IMapper mapper)
        {
            _loanRepository = loanRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<LenderFundingDto>>> Handle(GetFundingsByLenderQuery request, CancellationToken cancellationToken)
        {
            var loans = await _loanRepository.GetAllIncludingFundingsAsync(cancellationToken);

            var filteredLoans = loans
                .Where(loan => loan.Fundings.Any(f => f.LenderId == request.LenderId))
                .ToList();

            var result = filteredLoans
                        .Select(loan => _mapper.Map<LenderFundingDto>(loan, opt =>
                        {
                            opt.Items["LenderId"] = request.LenderId;
                        }))
                        .ToList();


            return ApiResponse<List<LenderFundingDto>>.SuccessResult(result);
        }
    }
}
