using FluentValidation;

namespace LoanWise.Application.Features.Loans.Queries.GetBorrowerLoanHistory
{
    public sealed class GetBorrowerLoanHistoryQueryValidator : AbstractValidator<GetBorrowerLoanHistoryQuery>
    {
        public GetBorrowerLoanHistoryQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
