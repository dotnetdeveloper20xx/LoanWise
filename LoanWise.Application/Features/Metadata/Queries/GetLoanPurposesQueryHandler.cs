using LoanWise.Application.DTOs.Metadata;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Metadata.Queries
{
    public sealed class GetLoanPurposesQueryHandler
        : IRequestHandler<GetLoanPurposesQuery, ApiResponse<List<EnumItemDto>>>
    {
        public Task<ApiResponse<List<EnumItemDto>>> Handle(GetLoanPurposesQuery request, CancellationToken ct)
        {
            var items = Enum.GetValues(typeof(LoanWise.Domain.Enums.LoanPurpose))
                .Cast<LoanWise.Domain.Enums.LoanPurpose>()
                .Select(e => new EnumItemDto(e.ToString(), (int)e))
                .ToList();

            return Task.FromResult(ApiResponse<List<EnumItemDto>>.SuccessResult(items));
        }
    }
}
