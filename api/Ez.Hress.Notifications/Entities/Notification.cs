namespace Ez.Hress.Notifications.Entities;

public class Notification
{
    /// <summary>Set by the data access on save.</summary>
    public string ID { get; set; } = "";
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Link { get; set; } = "";
    /// <summary>Null means the notification is for everyone.</summary>
    public int? RecipientUserID { get; set; }
    /// <summary>
    /// For a notification to everyone: a member who doesn't get it, e.g. the one whose action caused it.
    /// They don't see it in their list and get no push.
    /// </summary>
    public int? ExcludedUserID { get; set; }
    /// <summary>UTC.</summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// Optional stable identity. Together with RecipientUserID and Created it decides the storage key,
    /// so saving the same keyed notification twice is detected (see INotificationDataAccess.SaveIfNewAsync).
    /// </summary>
    public string? Key { get; set; }
}
