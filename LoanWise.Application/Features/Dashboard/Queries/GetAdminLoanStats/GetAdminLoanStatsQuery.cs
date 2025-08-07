
using LoanWise.Application.DTOs.Dashboard;

using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Dashboard.Queries.GetAdminLoanStats
{
    public class GetAdminLoanStatsQuery : IRequest<ApiResponse<AdminLoanStatsDto>> { }
}
