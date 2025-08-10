using LoanWise.Application.DTOs.Dashboard;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Dashboard.Queries.GetLenderPortfolio
{
    // No LenderId anymore; we read it from IUserContext in the handler
    public sealed class GetLenderPortfolioSummaryQuery
        : IRequest<ApiResponse<LenderPortfolioDto>>
    { }
}
