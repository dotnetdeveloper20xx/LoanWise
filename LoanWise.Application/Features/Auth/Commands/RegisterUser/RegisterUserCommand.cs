using LoanWise.Application.DTOs.Users;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Auth.Commands.RegisterUser
{
    public class RegisterUserCommand : IRequest<ApiResponse<Guid>>
    {
        public UserRegistrationDto Registration { get; set; } = default!;
    }
}
