using LoanWise.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Commands.UpdateUserStatus
{
    public sealed class UpdateUserStatusCommandHandler
          : IRequestHandler<UpdateUserStatusCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IUserContext _userContext; // To prevent admin from disabling themselves

        public UpdateUserStatusCommandHandler(IApplicationDbContext db, IUserContext userContext)
        {
            _db = db;
            _userContext = userContext;
        }

        public async Task<ApiResponse<bool>> Handle(UpdateUserStatusCommand request, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user is null)
                return ApiResponse<bool>.FailureResult("User not found.");

            // Prevent self-deactivation
            if (_userContext.UserId.HasValue && _userContext.UserId.Value == user.Id)
                return ApiResponse<bool>.FailureResult("You cannot deactivate your own account.");

            user.IsActive = request.IsActive;
            await _db.SaveChangesAsync(ct);

            return ApiResponse<bool>.SuccessResult(true, $"User {(request.IsActive ? "activated" : "deactivated")} successfully.");
        }
    }
}
