using Azure;
using Azure.Data.Tables;
using Ez.Hress.Notifications.Entities;

namespace Ez.Hress.Notifications.DataAccess;

internal class NotificationTableEntity : ITableEntity
{
    public NotificationTableEntity()
    {
        PartitionKey = string.Empty;
        RowKey = string.Empty;
    }

    public NotificationTableEntity(Notification notification)
    {
        PartitionKey = notification.RecipientUserID?.ToString() ?? NotificationKeys.AllPartition;
        RowKey = NotificationKeys.NewestFirst(notification.Created) + "_" + Guid.NewGuid().ToString("N");
        Type = notification.Type;
        Title = notification.Title;
        Link = notification.Link;
        RecipientUserID = notification.RecipientUserID;
        Created = DateTime.SpecifyKind(notification.Created, DateTimeKind.Utc);
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Link { get; set; } = "";
    public int? RecipientUserID { get; set; }
    public DateTime Created { get; set; }

    public string Id => $"{PartitionKey}|{RowKey}";

    public Notification ToNotification()
    {
        return new Notification
        {
            ID = Id,
            Type = Type,
            Title = Title,
            Link = Link,
            RecipientUserID = RecipientUserID,
            Created = DateTime.SpecifyKind(Created, DateTimeKind.Utc)
        };
    }
}
