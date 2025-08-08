using FluentValidation;

namespace LoanWise.Application.Features.Auth.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.Request.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
