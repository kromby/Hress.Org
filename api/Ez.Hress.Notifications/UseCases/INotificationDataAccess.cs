using Ez.Hress.Notifications.Entities;

namespace Ez.Hress.Notifications.UseCases;

public interface INotificationDataAccess
{
    /// <summary>Saves the notification and returns it with ID set.</summary>
    Task<Notification> SaveAsync(Notification notification);
    /// <summary>
    /// Saves a notification that has a Key. Returns it with ID set, or null when the same notification
    /// (same recipient, Created and Key) was already saved.
    /// </summary>
    Task<Notification?> SaveIfNewAsync(Notification notification);
    Task<Notification?> GetAsync(string id);
    /// <summary>
    /// Notifications for everyone plus those with RecipientUserID == userID, newest first.
    /// Leaves out notifications with ExcludedUserID == userID.
    /// </summary>
    Task<IList<Notification>> GetLatestAsync(int userID, int top);
}
