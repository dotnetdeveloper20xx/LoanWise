using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Commands.UpdateUserStatus
{
    public sealed record UpdateUserStatusCommand(Guid UserId, bool IsActive)
         : IRequest<ApiResponse<bool>>;
}
