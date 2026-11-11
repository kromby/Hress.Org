using Ez.Hress.Notifications.Entities;
using Ez.Hress.Notifications.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Notifications;

public class NotificationInteractorTests
{
    private readonly Mock<INotificationDataAccess> _notifications = new();
    private readonly Mock<INotificationSeenDataAccess> _seen = new();
    private readonly Mock<IPushSubscriptionDataAccess> _subscriptions = new();
    private readonly Mock<IPushQueue> _queue = new();
    private readonly NotificationInteractor _sut;

    public NotificationInteractorTests()
    {
        _notifications.Setup(n => n.SaveAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => { n.ID = "saved-id"; return n; });
        _sut = new NotificationInteractor(_notifications.Object, _seen.Object, _subscriptions.Object, _queue.Object,
            Mock.Of<ILogger<NotificationInteractor>>());
    }

    private static Notification N(DateTime created) => new() { ID = "x", Created = created };

    [Fact]
    public async Task NotifyNewsCreated_SavesNewsNotificationAndQueuesIt()
    {
        Notification? saved = null;
        _notifications.Setup(n => n.SaveAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => saved = n)
            .ReturnsAsync(new Notification { ID = "returned-id" });

        await _sut.NotifyNewsCreatedAsync(42, "Bjórkvöld");

        Assert.NotNull(saved);
        Assert.Equal("news", saved!.Type);
        Assert.Equal("Bjórkvöld", saved.Title);
        Assert.Equal("/news/42", saved.Link);
        Assert.Null(saved.RecipientUserID);
        Assert.True(Math.Abs((DateTime.UtcNow - saved.Created).TotalSeconds) < 5);
        _queue.Verify(q => q.EnqueueAsync("returned-id"), Times.Once);
    }

    [Fact]
    public async Task NotifyNewsCreated_TruncatesLongTitle()
    {
        Notification? saved = null;
        _notifications.Setup(n => n.SaveAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => saved = n)
            .ReturnsAsync(new Notification { ID = "id" });

        await _sut.NotifyNewsCreatedAsync(1, new string('a', 500));

        Assert.Equal(100, saved!.Title.Length);
        Assert.EndsWith("…", saved.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task NotifyNewsCreated_InvalidNewsID_Throws(int newsID)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.NotifyNewsCreatedAsync(newsID, "t"));
        _notifications.Verify(n => n.SaveAsync(It.IsAny<Notification>()), Times.Never);
        _queue.Verify(q => q.EnqueueAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetNotifications_ReturnsLastSeenBeforeThisVisit()
    {
        var lastSeen = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        _seen.Setup(s => s.GetLastSeenAsync(7)).ReturnsAsync(lastSeen);
        _notifications.Setup(n => n.GetLatestAsync(7, 50))
            .ReturnsAsync(new List<Notification> { N(DateTime.UtcNow), N(DateTime.UtcNow) });

        var result = await _sut.GetNotificationsAsync(7);

        Assert.Equal(lastSeen, result.LastSeen);
        Assert.Equal(2, result.Notifications.Count);
        _notifications.Verify(n => n.GetLatestAsync(7, 50), Times.Once);
    }

    [Fact]
    public async Task HasUnread_NoLastSeen_TrueWhenAnyNotification()
    {
        _seen.Setup(s => s.GetLastSeenAsync(7)).ReturnsAsync((DateTime?)null);
        _notifications.Setup(n => n.GetLatestAsync(7, 1)).ReturnsAsync(new List<Notification> { N(DateTime.UtcNow) });

        Assert.True(await _sut.HasUnreadAsync(7));
    }

    [Fact]
    public async Task HasUnread_NoLastSeen_FalseWhenEmpty()
    {
        _seen.Setup(s => s.GetLastSeenAsync(7)).ReturnsAsync((DateTime?)null);
        _notifications.Setup(n => n.GetLatestAsync(7, 1)).ReturnsAsync(new List<Notification>());

        Assert.False(await _sut.HasUnreadAsync(7));
    }

    [Fact]
    public async Task HasUnread_TrueOnlyWhenNewestIsAfterLastSeen()
    {
        var lastSeen = new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);
        _seen.Setup(s => s.GetLastSeenAsync(7)).ReturnsAsync(lastSeen);

        _notifications.Setup(n => n.GetLatestAsync(7, 1)).ReturnsAsync(new List<Notification> { N(lastSeen) });
        Assert.False(await _sut.HasUnreadAsync(7));

        _notifications.Setup(n => n.GetLatestAsync(7, 1)).ReturnsAsync(new List<Notification> { N(lastSeen.AddSeconds(1)) });
        Assert.True(await _sut.HasUnreadAsync(7));
    }

    [Fact]
    public async Task MarkSeen_SetsLastSeenToNow()
    {
        DateTime? set = null;
        _seen.Setup(s => s.SetLastSeenAsync(7, It.IsAny<DateTime>()))
            .Callback<int, DateTime>((_, t) => set = t)
            .Returns(Task.CompletedTask);

        await _sut.MarkSeenAsync(7);

        Assert.NotNull(set);
        Assert.True(Math.Abs((DateTime.UtcNow - set!.Value).TotalSeconds) < 5);
    }

    [Fact]
    public async Task SaveSubscription_UpsertsWithCreatedSet()
    {
        var upserted = new List<PushSubscription>();
        _subscriptions.Setup(s => s.UpsertAsync(It.IsAny<PushSubscription>()))
            .Callback<PushSubscription>(upserted.Add)
            .Returns(Task.CompletedTask);

        await _sut.SaveSubscriptionAsync(new PushSubscription { Endpoint = "e", P256dh = "p", Auth = "a", UserID = 1 });
        await _sut.SaveSubscriptionAsync(new PushSubscription { Endpoint = "e", P256dh = "p", Auth = "a", UserID = 2 });

        Assert.Equal(2, upserted.Count);
        Assert.All(upserted, s => Assert.True(Math.Abs((DateTime.UtcNow - s.Created).TotalSeconds) < 5));
        Assert.Equal(2, upserted[1].UserID);
    }

    [Theory]
    [InlineData("", "p", "a")]
    [InlineData("e", "", "a")]
    [InlineData("e", "p", "")]
    public async Task SaveSubscription_MissingFields_Throws(string endpoint, string p256dh, string auth)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SaveSubscriptionAsync(
            new PushSubscription { Endpoint = endpoint, P256dh = p256dh, Auth = auth, UserID = 1 }));
        _subscriptions.Verify(s => s.UpsertAsync(It.IsAny<PushSubscription>()), Times.Never);
    }

    [Fact]
    public async Task RemoveSubscription_OnlyOwner()
    {
        _subscriptions.Setup(s => s.GetAsync("e")).ReturnsAsync(new PushSubscription { Endpoint = "e", UserID = 5 });

        await _sut.RemoveSubscriptionAsync(6, "e");
        _subscriptions.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);

        await _sut.RemoveSubscriptionAsync(5, "e");
        _subscriptions.Verify(s => s.DeleteAsync("e"), Times.Once);

        _subscriptions.Setup(s => s.GetAsync("gone")).ReturnsAsync((PushSubscription?)null);
        await _sut.RemoveSubscriptionAsync(5, "gone");
        _subscriptions.Verify(s => s.DeleteAsync("gone"), Times.Never);
    }

    [Fact]
    public async Task IsSubscribed_TrueOnlyForOwner()
    {
        _subscriptions.Setup(s => s.GetAsync("e")).ReturnsAsync(new PushSubscription { Endpoint = "e", UserID = 5 });
        _subscriptions.Setup(s => s.GetAsync("none")).ReturnsAsync((PushSubscription?)null);

        Assert.True(await _sut.IsSubscribedAsync(5, "e"));
        Assert.False(await _sut.IsSubscribedAsync(6, "e"));
        Assert.False(await _sut.IsSubscribedAsync(5, "none"));
    }
}
