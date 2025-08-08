using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Notifications.Commands.MarkNotificationAsRead
{
    public sealed record MarkNotificationAsReadCommand(Guid NotificationId)
         : IRequest<ApiResponse<bool>>;
}
