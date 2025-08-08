using LoanWise.Application.DTOs.Metadata;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Metadata.Queries
{
    public sealed class GetLoanStatusesQueryHandler
     : IRequestHandler<GetLoanStatusesQuery, ApiResponse<List<EnumItemDto>>>
    {
        public Task<ApiResponse<List<EnumItemDto>>> Handle(GetLoanStatusesQuery request, CancellationToken ct)
        {
            var items = Enum.GetValues(typeof(LoanWise.Domain.Enums.LoanStatus))
                .Cast<LoanWise.Domain.Enums.LoanStatus>()
                .Select(e => new EnumItemDto(e.ToString(), (int)e))
                .ToList();

            return Task.FromResult(ApiResponse<List<EnumItemDto>>.SuccessResult(items));
        }
    }
}
