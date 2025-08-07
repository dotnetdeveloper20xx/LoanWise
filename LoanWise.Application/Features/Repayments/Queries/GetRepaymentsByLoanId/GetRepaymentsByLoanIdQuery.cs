using LoanWise.Application.DTOs.Repayments;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Repayments.Queries.GetRepaymentsByLoanId
{
    public class GetRepaymentsByLoanIdQuery : IRequest<ApiResponse<List<RepaymentDto>>>
    {
        public Guid LoanId { get; }

        public GetRepaymentsByLoanIdQuery(Guid loanId)
        {
            LoanId = loanId;
        }
    }
}
