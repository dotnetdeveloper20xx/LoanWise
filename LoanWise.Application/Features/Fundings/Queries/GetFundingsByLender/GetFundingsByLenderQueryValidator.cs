using FluentValidation;

namespace LoanWise.Application.Features.Fundings.Queries.GetFundingsByLender
{
    public sealed class GetFundingsByLenderQueryValidator : AbstractValidator<GetFundingsByLenderQuery>
    {
        public GetFundingsByLenderQueryValidator()
        {
            // No specific rules now, but kept for future filters (date range, min/max amounts)
        }
    }
}
