using FluentValidation;

namespace LoanWise.Application.Features.Auth.Commands.LoginUser
{
    public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(x => x.Login).NotNull();

            When(x => x.Login != null, () =>
            {
                RuleFor(x => x.Login.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Login.Password).NotEmpty();
            });
        }
    }
}
