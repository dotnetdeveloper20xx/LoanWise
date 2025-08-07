
using LoanWise.Application.Features.Fundings.DTOs;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Fundings.Queries.GetFundingsByLender
{
    public class GetFundingsByLenderQuery : IRequest<ApiResponse<List<LenderFundingDto>>>
    {
        public Guid LenderId { get; }

        public GetFundingsByLenderQuery(Guid lenderId)
        {
            LenderId = lenderId;
        }
    }
}
