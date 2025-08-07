using LoanWise.Application.Features.Loans.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Queries.GetLoansByBorrower
{
    /// <summary>
    /// Query to fetch loans for the currently authenticated borrower.
    /// </summary>
    public class GetLoansByBorrowerQuery : IRequest<ApiResponse<List<BorrowerLoanDto>>>
    {
        // No parameters needed; UserId is resolved via IUserContext in the handler
    }
}
