using FluentValidation;

namespace LoanWise.Application.Features.Fundings.Commands.FundLoan
{
    /// <summary>
    /// Validates the FundLoanCommand input.
    /// </summary>
    public sealed class FundLoanCommandValidator : AbstractValidator<FundLoanCommand>
    {
        public FundLoanCommandValidator()
        {
            RuleFor(x => x.LoanId).NotEmpty();
            RuleFor(x => x.Amount)
                .GreaterThan(0m).WithMessage("Amount must be greater than 0.");
            // NOTE: Do NOT check “<= remaining” here anymore — we clamp in the handler.
        }
    }
}
