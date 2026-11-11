namespace Ez.Hress.Notifications.Entities;

public class PushSubscription
{
    public string Endpoint { get; set; } = "";
    public string P256dh { get; set; } = "";
    public string Auth { get; set; } = "";
    public int UserID { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Created { get; set; }
}
