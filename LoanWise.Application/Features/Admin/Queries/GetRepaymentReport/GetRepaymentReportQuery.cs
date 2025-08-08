using LoanWise.Application.DTOs.Reports;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Queries.GetRepaymentReport
{
    public sealed record GetRepaymentReportQuery()
     : IRequest<ApiResponse<List<RepaymentReportDto>>>;
}
