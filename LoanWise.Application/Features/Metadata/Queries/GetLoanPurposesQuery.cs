using LoanWise.Application.DTOs.Metadata;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Metadata.Queries
{
    public sealed record GetLoanPurposesQuery()
         : IRequest<ApiResponse<List<EnumItemDto>>>;
}
