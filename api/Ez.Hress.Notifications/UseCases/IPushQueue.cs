namespace Ez.Hress.Notifications.UseCases;

public interface IPushQueue
{
    Task EnqueueAsync(string notificationID);
}
