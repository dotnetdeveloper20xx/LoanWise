using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Notifications.Queries.GetNotifications
{
    public sealed class GetNotificationsQueryHandler
        : IRequestHandler<GetNotificationsQuery, ApiResponse<List<NotificationDto>>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IUserContext _userContext;

        public GetNotificationsQueryHandler(IApplicationDbContext db, IUserContext userContext)
        {
            _db = db;
            _userContext = userContext;
        }

        public async Task<ApiResponse<List<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken ct)
        {
            if (!_userContext.UserId.HasValue)
                return ApiResponse<List<NotificationDto>>.FailureResult("Unauthorized");

            var notifications = await _db.Notifications
                .Where(n => n.UserId == _userContext.UserId.Value)
                .OrderByDescending(n => n.CreatedAtUtc)
                .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.IsRead, n.CreatedAtUtc))
                .ToListAsync(ct);

            return ApiResponse<List<NotificationDto>>.SuccessResult(notifications);
        }
    }
}
