namespace Ez.Hress.Notifications.UseCases;

public interface INotificationSeenDataAccess
{
    Task<DateTime?> GetLastSeenAsync(int userID);
    Task SetLastSeenAsync(int userID, DateTime seen);
}
