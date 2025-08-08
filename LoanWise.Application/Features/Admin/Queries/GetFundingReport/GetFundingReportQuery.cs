using LoanWise.Application.DTOs.Reports;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Queries.GetFundingReport
{
    public sealed record GetFundingReportQuery()
        : IRequest<ApiResponse<List<FundingReportDto>>>;
}
