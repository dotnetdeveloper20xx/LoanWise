using LoanWise.Application.Common.Interfaces;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Auth.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ApiResponse<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<ApiResponse<string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Login;

            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user is null)
                return ApiResponse<string>.FailureResult("Invalid email or password.");

            var isPasswordValid = _passwordService.VerifyPassword(user.PasswordHash, dto.Password, user);
            if (!isPasswordValid)
                return ApiResponse<string>.FailureResult("Invalid email or password.");

            var token = _jwtTokenGenerator.GenerateToken(user);
            return ApiResponse<string>.SuccessResult(token, "Login successful.");
        }
    }
}
