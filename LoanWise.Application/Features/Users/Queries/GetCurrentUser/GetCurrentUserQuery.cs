using LoanWise.Application.DTOs.Users;
using StoreBoost.Application.Common.Models;
using MediatR;

namespace LoanWise.Application.Features.Users.Queries.GetCurrentUser
{
    public record GetCurrentUserQuery(Guid UserId) : IRequest<ApiResponse<UserProfileDto>>;
}
