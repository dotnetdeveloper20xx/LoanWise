using LoanWise.Infrastructure.Notifications; // EmailNotificationService

public sealed class CompositeNotificationService : INotificationService
{
    private readonly SignalRNotificationService _signalr;
    private readonly EmailNotificationService _email;

    public CompositeNotificationService(SignalRNotificationService signalr, EmailNotificationService email)
    {
        _signalr = signalr;
        _email = email;
    }

    public Task NotifyBorrowerAsync(Guid borrowerId, string title, string message, CancellationToken ct = default) =>
        Task.WhenAll(
            _signalr.NotifyBorrowerAsync(borrowerId, title, message, ct),
            _email.NotifyBorrowerAsync(borrowerId, title, message, ct));

    public Task NotifyLenderAsync(Guid lenderId, string title, string message, CancellationToken ct = default) =>
        Task.WhenAll(
            _signalr.NotifyLenderAsync(lenderId, title, message, ct),
            _email.NotifyLenderAsync(lenderId, title, message, ct));
}
