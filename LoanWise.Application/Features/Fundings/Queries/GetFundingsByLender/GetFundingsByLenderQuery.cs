
using LoanWise.Application.Features.Fundings.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Fundings.Queries.GetFundingsByLender
{
    public sealed record GetFundingsByLenderQuery()
        : IRequest<ApiResponse<List<LenderFundingDto>>>;
}
