using LoanWise.Application.DTOs.Metadata;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Metadata.Queries
{
    public sealed class GetRiskLevelsQueryHandler
        : IRequestHandler<GetRiskLevelsQuery, ApiResponse<List<EnumItemDto>>>
    {
        public Task<ApiResponse<List<EnumItemDto>>> Handle(GetRiskLevelsQuery request, CancellationToken ct)
        {
            var items = Enum.GetValues(typeof(LoanWise.Domain.Enums.RiskLevel))
                .Cast<LoanWise.Domain.Enums.RiskLevel>()
                .Select(e => new EnumItemDto(e.ToString(), (int)e))
                .ToList();

            return Task.FromResult(ApiResponse<List<EnumItemDto>>.SuccessResult(items));
        }
    }
}
