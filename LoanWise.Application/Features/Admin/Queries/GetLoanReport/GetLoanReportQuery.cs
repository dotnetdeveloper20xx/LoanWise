using LoanWise.Application.DTOs.Reports;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Queries.GetLoanReport
{
    public sealed record GetLoanReportQuery()
         : IRequest<ApiResponse<List<LoanReportDto>>>;
}
