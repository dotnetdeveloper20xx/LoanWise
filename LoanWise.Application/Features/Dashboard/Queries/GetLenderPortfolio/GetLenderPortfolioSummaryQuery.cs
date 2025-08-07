using LoanWise.Application.DTOs.Dashboard;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Dashboard.Queries.GetLenderPortfolio
{
    public class GetLenderPortfolioSummaryQuery : IRequest<ApiResponse<LenderPortfolioDto>>
    {
        public Guid LenderId { get; }

        public GetLenderPortfolioSummaryQuery(Guid lenderId)
        {
            LenderId = lenderId;
        }
    }
}
