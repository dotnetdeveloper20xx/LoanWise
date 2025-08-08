using AutoMapper;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Dashboard;
using LoanWise.Domain.Enums;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Queries.GetPendingLoans
{
    public class GetPendingLoansQueryHandler : IRequestHandler<GetPendingLoansQuery, ApiResponse<List<AdminLoanListItemDto>>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;

        public GetPendingLoansQueryHandler(ILoanRepository loanRepository, IMapper mapper)
        {
            _loanRepository = loanRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<AdminLoanListItemDto>>> Handle(GetPendingLoansQuery request, CancellationToken cancellationToken)
        {
            var loans = await _loanRepository.GetByStatusAsync(LoanStatus.Pending, cancellationToken);
            var dto = _mapper.Map<List<AdminLoanListItemDto>>(loans);
            return ApiResponse<List<AdminLoanListItemDto>>.SuccessResult(dto);
        }
    }
}
