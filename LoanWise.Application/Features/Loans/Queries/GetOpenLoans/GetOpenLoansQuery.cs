
using LoanWise.Application.Features.Loans.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Queries.GetOpenLoans
{
    public class GetOpenLoansQuery : IRequest<ApiResponse<List<LoanSummaryDto>>> { }
}
