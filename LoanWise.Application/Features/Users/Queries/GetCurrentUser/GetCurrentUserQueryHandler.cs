using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Users;
using StoreBoost.Application.Common.Models;
using MediatR;

namespace LoanWise.Application.Features.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ApiResponse<UserProfileDto>>
    {
        private readonly IUserRepository _userRepository;

        public GetCurrentUserQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<UserProfileDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user is null)
                return ApiResponse<UserProfileDto>.FailureResult("User not found");

            var dto = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                CreditScore = user.CreditScore,
                RiskTier = user.Tier,
                KycVerified = user.KycVerified
            };

            return ApiResponse<UserProfileDto>.SuccessResult(dto);
        }
    }
}
