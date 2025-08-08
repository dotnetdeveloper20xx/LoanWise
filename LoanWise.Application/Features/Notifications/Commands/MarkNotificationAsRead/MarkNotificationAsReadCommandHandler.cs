using LoanWise.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Notifications.Commands.MarkNotificationAsRead
{
    public sealed class MarkNotificationAsReadCommandHandler
        : IRequestHandler<MarkNotificationAsReadCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IUserContext _userContext;

        public MarkNotificationAsReadCommandHandler(IApplicationDbContext db, IUserContext userContext)
        {
            _db = db;
            _userContext = userContext;
        }

        public async Task<ApiResponse<bool>> Handle(MarkNotificationAsReadCommand request, CancellationToken ct)
        {
            if (!_userContext.UserId.HasValue)
                return ApiResponse<bool>.FailureResult("Unauthorized");

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == _userContext.UserId.Value, ct);

            if (notification is null)
                return ApiResponse<bool>.FailureResult("Notification not found");

            notification.IsRead = true;
            await _db.SaveChangesAsync(ct);

            return ApiResponse<bool>.SuccessResult(true, "Notification marked as read");
        }
    }
}
