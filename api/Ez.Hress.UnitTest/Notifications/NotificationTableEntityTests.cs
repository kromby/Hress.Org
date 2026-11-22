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

    [Theory]
    [InlineData(7)]
    [InlineData(null)]
    public void ExcludedUserID_RoundTrips(int? excluded)
    {
        var notification = new Notification { Type = "beer", Title = "Jóla Gull · Ölgerðin", Link = "/beer/abc", ExcludedUserID = excluded, Created = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc) };

        var entity = new NotificationTableEntity(notification);

        Assert.Equal(excluded, entity.ExcludedUserID);
        Assert.Equal(excluded, entity.ToNotification().ExcludedUserID);
    }

    private static async IAsyncEnumerable<NotificationTableEntity> Rows(params int?[] excluded)
    {
        var created = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc);
        for (var i = 0; i < excluded.Length; i++)
        {
            await Task.Yield();
            yield return new NotificationTableEntity(new Notification { Type = "beer", Title = $"n{i}", ExcludedUserID = excluded[i], Created = created.AddMinutes(-i) });
        }
    }

    [Fact]
    public async Task TakeVisible_SkipsRowsExcludingTheUser_WithoutCountingThem()
    {
        var result = await NotificationTableAccess.TakeVisibleAsync(Rows(7, null, 7, 8, null), 7, 2);

        Assert.Equal(new[] { "n1", "n3" }, result.Select(n => n.Title));
    }

    [Fact]
    public async Task TakeVisible_OtherUser_SeesExcludedRows()
    {
        var result = await NotificationTableAccess.TakeVisibleAsync(Rows(7, null, 7), 8, 10);

        Assert.Equal(new[] { "n0", "n1", "n2" }, result.Select(n => n.Title));
    }
}
