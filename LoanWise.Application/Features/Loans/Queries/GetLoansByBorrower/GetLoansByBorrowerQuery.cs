
using LoanWise.Application.Features.Loans.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Queries.GetLoansByBorrower
{
    public class GetLoansByBorrowerQuery : IRequest<ApiResponse<List<BorrowerLoanDto>>>
    {
        public Guid BorrowerId { get; }

        public GetLoansByBorrowerQuery(Guid borrowerId)
        {
            BorrowerId = borrowerId;
        }
    }
}
