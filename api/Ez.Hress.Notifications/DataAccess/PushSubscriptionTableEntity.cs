using Azure;
using Azure.Data.Tables;
using Ez.Hress.Notifications.Entities;

namespace Ez.Hress.Notifications.DataAccess;

internal class PushSubscriptionTableEntity : ITableEntity
{
    public PushSubscriptionTableEntity()
    {
        PartitionKey = NotificationKeys.AllPartition;
        RowKey = string.Empty;
    }

    public PushSubscriptionTableEntity(PushSubscription subscription)
    {
        PartitionKey = NotificationKeys.AllPartition;
        RowKey = NotificationKeys.EndpointKey(subscription.Endpoint);
        Endpoint = subscription.Endpoint;
        P256dh = subscription.P256dh;
        Auth = subscription.Auth;
        UserID = subscription.UserID;
        UserAgent = subscription.UserAgent;
        Created = DateTime.SpecifyKind(subscription.Created, DateTimeKind.Utc);
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Endpoint { get; set; } = "";
    public string P256dh { get; set; } = "";
    public string Auth { get; set; } = "";
    public int UserID { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Created { get; set; }

    public PushSubscription ToSubscription()
    {
        return new PushSubscription
        {
            Endpoint = Endpoint,
            P256dh = P256dh,
            Auth = Auth,
            UserID = UserID,
            UserAgent = UserAgent,
            Created = DateTime.SpecifyKind(Created, DateTimeKind.Utc)
        };
    }
}
