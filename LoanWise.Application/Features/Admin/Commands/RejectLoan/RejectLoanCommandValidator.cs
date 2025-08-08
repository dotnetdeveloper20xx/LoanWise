using FluentValidation;

namespace LoanWise.Application.Features.Admin.Commands.RejectLoan
{
    public class RejectLoanCommandValidator : AbstractValidator<RejectLoanCommand>
    {
        public RejectLoanCommandValidator()
        {
            RuleFor(x => x.LoanId).NotEmpty();
            RuleFor(x => x.Reason).MaximumLength(500);
        }
    }
}
