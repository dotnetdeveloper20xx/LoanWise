using LoanWise.Application.DTOs.Users;
using StoreBoost.Application.Common.Models;
using MediatR;

namespace LoanWise.Application.Features.Auth.Commands.LoginUser
{
    public class LoginUserCommand : IRequest<ApiResponse<string>>
    {
        public LoginDto Login { get; set; } = default!;
    }
}
