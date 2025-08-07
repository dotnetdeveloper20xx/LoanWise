using FluentValidation;

namespace LoanWise.Application.Features.Loans.Commands.ApplyLoan
{
    /// <summary>
    /// Validates input for ApplyLoanCommand.
    /// </summary>
    public class ApplyLoanCommandValidator : AbstractValidator<ApplyLoanCommand>
    {
        public ApplyLoanCommandValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Loan amount must be greater than 0.");

            RuleFor(x => x.DurationInMonths)
                .InclusiveBetween(1, 120);

            RuleFor(x => x.Purpose)
                .IsInEnum();

            RuleFor(x => x.BorrowerId)
                .NotEmpty().WithMessage("Borrower ID is required.");
        }
    }
}
