using FluentValidation;

namespace LoanWise.Application.Features.Admin.Commands.ApproveLoan
{
    public class ApproveLoanCommandValidator : AbstractValidator<ApproveLoanCommand>
    {
        public ApproveLoanCommandValidator()
        {
            RuleFor(x => x.LoanId).NotEmpty();
        }
    }
}
