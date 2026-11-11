using Ez.Hress.Notifications.Entities;

namespace Ez.Hress.Notifications.UseCases;

public interface INotificationDataAccess
{
    /// <summary>Saves the notification and returns it with ID set.</summary>
    Task<Notification> SaveAsync(Notification notification);
    Task<Notification?> GetAsync(string id);
    /// <summary>Notifications for everyone plus those with RecipientUserID == userID, newest first.</summary>
    Task<IList<Notification>> GetLatestAsync(int userID, int top);
}
