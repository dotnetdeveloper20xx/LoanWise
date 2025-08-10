public interface INotificationsClient
{
    Task ReceiveNotification(string title, string message);
}