using LoanWise.Application.DTOs.Dashboard;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Queries.GetPendingLoans
{
    public class GetPendingLoansQuery : IRequest<ApiResponse<List<AdminLoanListItemDto>>> { }
}
