using LoanWise.Application.DTOs.Loans;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Queries.GetBorrowerLoanHistory
{
    public sealed record GetBorrowerLoanHistoryQuery(int Page = 1, int PageSize = 20)
          : IRequest<ApiResponse<List<BorrowerLoanHistoryDto>>>;
}
