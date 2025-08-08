using LoanWise.Application.DTOs.Notifications;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Notifications.Queries.GetNotifications
{
    public sealed record GetNotificationsQuery()
         : IRequest<ApiResponse<List<NotificationDto>>>;
}
