
using LoanWise.Application.DTOs.Dashboard;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Dashboard.Queries.GetBorrowerDashboard
{
    public class GetBorrowerDashboardQuery : IRequest<ApiResponse<BorrowerDashboardDto>>
    {
        public Guid BorrowerId { get; }

        public GetBorrowerDashboardQuery(Guid borrowerId)
        {
            BorrowerId = borrowerId;
        }
    }
}
