using FluentValidation;

namespace LoanWise.Application.Features.Fundings.Commands.FundLoan
{
    /// <summary>
    /// Validates the FundLoanCommand input.
    /// </summary>
    public class FundLoanCommandValidator : AbstractValidator<FundLoanCommand>
    {
        public FundLoanCommandValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Funding amount must be greater than zero.");

            RuleFor(x => x.LoanId)
                .NotEmpty().WithMessage("Loan ID is required.");

            //Fazi, get this from the user context. Security issue.

            //RuleFor(x => x.LenderId)
            //    .NotEmpty().WithMessage("Lender ID is required.");
        }
    }
}
