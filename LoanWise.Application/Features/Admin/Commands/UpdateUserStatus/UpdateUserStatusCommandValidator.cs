using FluentValidation;

namespace LoanWise.Application.Features.Admin.Commands.UpdateUserStatus
{
    public sealed class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
    {
        public UpdateUserStatusCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
