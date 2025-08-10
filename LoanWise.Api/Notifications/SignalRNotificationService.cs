// LoanWise.Api/Notifications/SignalRNotificationService.cs
using Microsoft.AspNetCore.SignalR;

public sealed class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationsHub, INotificationsClient> _hub;

    public SignalRNotificationService(IHubContext<NotificationsHub, INotificationsClient> hub)
        => _hub = hub;

    public Task NotifyBorrowerAsync(Guid borrowerId, string title, string message, CancellationToken ct = default) =>
        _hub.Clients.User(borrowerId.ToString()).ReceiveNotification(title, message);

    public Task NotifyLenderAsync(Guid lenderId, string title, string message, CancellationToken ct = default) =>
        _hub.Clients.User(lenderId.ToString()).ReceiveNotification(title, message);
}
