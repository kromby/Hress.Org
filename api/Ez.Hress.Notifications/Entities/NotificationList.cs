namespace Ez.Hress.Notifications.Entities;

public class NotificationList
{
    public DateTime? LastSeen { get; set; }
    public IList<Notification> Notifications { get; set; } = [];
}
