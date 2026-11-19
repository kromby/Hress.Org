using Ez.Hress.Notifications.DataAccess;
using Ez.Hress.Notifications.Entities;

namespace Ez.Hress.UnitTest.Notifications;

public class NotificationTableEntityTests
{
    private static Notification Hardhead(int recipient) => new()
    {
        Type = "hardhead",
        RecipientUserID = recipient,
        Created = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc),
        Key = "hardhead-123"
    };

    [Fact]
    public void KeyedNotification_SameRecipientCreatedAndKey_SameStorageKeys()
    {
        var first = new NotificationTableEntity(Hardhead(7));
        var second = new NotificationTableEntity(Hardhead(7));

        Assert.Equal("7", first.PartitionKey);
        Assert.Equal(first.RowKey, second.RowKey);
        Assert.EndsWith("_hardhead-123", first.RowKey);
    }

    [Fact]
    public void KeyedNotification_DifferentRecipient_DifferentPartition()
    {
        Assert.NotEqual(new NotificationTableEntity(Hardhead(7)).Id, new NotificationTableEntity(Hardhead(8)).Id);
    }

    [Fact]
    public void UnkeyedNotification_GetsUniqueRowKey()
    {
        var notification = new Notification { Type = "news", Created = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc) };

        Assert.NotEqual(new NotificationTableEntity(notification).RowKey, new NotificationTableEntity(notification).RowKey);
        Assert.Equal("all", new NotificationTableEntity(notification).PartitionKey);
    }
}
