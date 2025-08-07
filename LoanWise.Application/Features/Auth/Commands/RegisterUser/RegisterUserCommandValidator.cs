using FluentValidation;

namespace LoanWise.Application.Features.Auth.Commands.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Registration).NotNull().WithMessage("Registration data is required");

            When(x => x.Registration != null, () =>
            {
                RuleFor(x => x.Registration.FullName).NotEmpty().MinimumLength(2);
                RuleFor(x => x.Registration.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Registration.Password).NotEmpty().MinimumLength(6);
                RuleFor(x => x.Registration.Role).IsInEnum();
            });
        }
    }
}
