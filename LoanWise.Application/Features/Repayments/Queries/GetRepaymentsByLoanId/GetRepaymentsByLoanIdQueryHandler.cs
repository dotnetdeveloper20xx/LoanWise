// Application/Features/Loans/Queries/GetRepaymentsByLoanId/GetRepaymentsByLoanIdQueryHandler.cs
using AutoMapper;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Repayments;
using LoanWise.Application.Features.Repayments.Queries.GetRepaymentsByLoanId;
using LoanWise.Domain.Enums;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Queries.GetRepaymentsByLoanId
{
    public class GetRepaymentsByLoanIdQueryHandler
        : IRequestHandler<GetRepaymentsByLoanIdQuery, ApiResponse<List<RepaymentDto>>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;

        public GetRepaymentsByLoanIdQueryHandler(ILoanRepository loanRepository, IMapper mapper)
        {
            _loanRepository = loanRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<RepaymentDto>>> Handle(GetRepaymentsByLoanIdQuery request, CancellationToken ct)
        {
            // Must include Repayments to avoid lazy-loading issues
            var loan = await _loanRepository.GetByIdWithRepaymentsAsync(request.LoanId, ct);
            if (loan is null)
                return ApiResponse<List<RepaymentDto>>.FailureResult("Loan not found.");

            if (loan.Status != LoanStatus.Disbursed)
                return ApiResponse<List<RepaymentDto>>.FailureResult("Loan is not disbursed yet; no repayment schedule exists.");

            var now = DateTime.UtcNow;

            // Order first, then map; pass "now" so the profile can compute IsOverdue/Status consistently
            var ordered = loan.Repayments
                .OrderBy(r => r.DueDate)
                .ToList();

            var result = _mapper.Map<List<RepaymentDto>>(ordered, opts =>
            {
                opts.Items["now"] = now;
            });

            return ApiResponse<List<RepaymentDto>>.SuccessResult(result);
        }
    }
}
