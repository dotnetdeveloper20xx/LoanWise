// Application/Common/Interfaces/INotificationService.cs
public interface INotificationService
{
    Task NotifyBorrowerAsync(Guid borrowerId, string title, string message, CancellationToken ct = default);
    Task NotifyLenderAsync(Guid lenderId, string title, string message, CancellationToken ct = default);
}

