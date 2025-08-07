using LoanWise.Application.DTOs.Dashboard;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Dashboard.Queries.GetBorrowerDashboard
{
    /// <summary>
    /// Query to fetch borrower dashboard data for the authenticated user.
    /// UserId will be resolved from IUserContext.
    /// </summary>
    public class GetBorrowerDashboardQuery : IRequest<ApiResponse<BorrowerDashboardDto>>
    {
        // No parameters needed — UserId will be pulled from the context
    }
}
