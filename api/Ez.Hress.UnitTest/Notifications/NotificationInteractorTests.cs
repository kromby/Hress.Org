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

    private List<Notification> CaptureHardheadSaves(Func<Notification, bool>? alreadySaved = null)
    {
        var saved = new List<Notification>();
        _notifications.Setup(n => n.SaveIfNewAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) =>
            {
                if (alreadySaved?.Invoke(n) == true)
                {
                    return null;
                }
                saved.Add(n);
                n.ID = $"{n.RecipientUserID}|{n.Key}";
                return n;
            });
        return saved;
    }

    [Fact]
    public async Task NotifyHardheadTonight_OneNotificationPerRecipient_QueuedAndStampedAtNine()
    {
        var saved = CaptureHardheadSaves();
        var day = new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc);

        var queued = await _sut.NotifyHardheadTonightAsync(123, 250, "Jónsi", day, new[] { 7, 8 });

        Assert.Equal(2, queued);
        Assert.Equal(new int?[] { 7, 8 }, saved.Select(n => n.RecipientUserID));
        Assert.All(saved, n =>
        {
            Assert.Equal("hardhead", n.Type);
            Assert.Equal("Harðhaus #250 er í kvöld hjá Jónsi", n.Title);
            Assert.Equal("/hardhead/123", n.Link);
            Assert.Equal("hardhead-123", n.Key);
            Assert.Equal(new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc), n.Created);
            Assert.Equal(DateTimeKind.Utc, n.Created.Kind);
        });
        _queue.Verify(q => q.EnqueueAsync("7|hardhead-123"), Times.Once);
        _queue.Verify(q => q.EnqueueAsync("8|hardhead-123"), Times.Once);
    }

    [Fact]
    public async Task NotifyHardheadTonight_StampIgnoresTimeOfDay()
    {
        var saved = CaptureHardheadSaves();

        await _sut.NotifyHardheadTonightAsync(1, 1, "a", new DateTime(2026, 11, 20, 9, 0, 3, DateTimeKind.Utc), new[] { 7 });
        await _sut.NotifyHardheadTonightAsync(1, 1, "a", new DateTime(2026, 11, 20, 14, 30, 0, DateTimeKind.Utc), new[] { 7 });

        Assert.Equal(saved[0].Created, saved[1].Created);
    }

    [Fact]
    public async Task NotifyHardheadTonight_AlreadyNotified_NotQueuedAgain()
    {
        CaptureHardheadSaves(alreadySaved: n => n.RecipientUserID == 7);

        var queued = await _sut.NotifyHardheadTonightAsync(123, 250, "Jónsi", new DateTime(2026, 11, 20), new[] { 7, 8 });

        Assert.Equal(1, queued);
        _queue.Verify(q => q.EnqueueAsync("8|hardhead-123"), Times.Once);
        _queue.Verify(q => q.EnqueueAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task NotifyHardheadTonight_DuplicateAndInvalidRecipients_Skipped()
    {
        var saved = CaptureHardheadSaves();

        await _sut.NotifyHardheadTonightAsync(123, 250, "Jónsi", new DateTime(2026, 11, 20), new[] { 7, 7, 0, -1, 8 });

        Assert.Equal(new int?[] { 7, 8 }, saved.Select(n => n.RecipientUserID));
    }

    [Fact]
    public async Task NotifyHardheadTonight_NoHostName_TitleWithoutHost()
    {
        var saved = CaptureHardheadSaves();

        await _sut.NotifyHardheadTonightAsync(123, 250, " ", new DateTime(2026, 11, 20), new[] { 7 });

        Assert.Equal("Harðhaus #250 er í kvöld", saved.Single().Title);
    }

    [Theory]
    [InlineData(20, 0, "Bláskógar 3", "Harðhaus #250 er í kvöld kl. 20:00 hjá Jónsi · Bláskógar 3")]
    [InlineData(19, 30, null, "Harðhaus #250 er í kvöld kl. 19:30 hjá Jónsi")]
    [InlineData(null, null, " Bláskógar 3 ", "Harðhaus #250 er í kvöld hjá Jónsi · Bláskógar 3")]
    public async Task NotifyHardheadTonight_TitleWithStartTimeAndLocation(int? hour, int? minute, string? location, string expected)
    {
        var saved = CaptureHardheadSaves();
        TimeSpan? startTime = hour.HasValue ? new TimeSpan(hour.Value, minute!.Value, 0) : null;

        await _sut.NotifyHardheadTonightAsync(123, 250, "Jónsi", new DateTime(2026, 11, 20), new[] { 7 }, startTime, location);

        Assert.Equal(expected, saved.Single().Title);
    }

    [Fact]
    public async Task NotifyHardheadTonight_NoRecipients_NothingSaved()
    {
        var queued = await _sut.NotifyHardheadTonightAsync(123, 250, "Jónsi", new DateTime(2026, 11, 20), Array.Empty<int>());

        Assert.Equal(0, queued);
        _notifications.Verify(n => n.SaveIfNewAsync(It.IsAny<Notification>()), Times.Never);
        _queue.Verify(q => q.EnqueueAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task NotifyHardheadTonight_InvalidHardheadID_Throws(int hardheadID)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.NotifyHardheadTonightAsync(hardheadID, 1, "a", new DateTime(2026, 11, 20), new[] { 7 }));
        _notifications.Verify(n => n.SaveIfNewAsync(It.IsAny<Notification>()), Times.Never);
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

        await _sut.SaveSubscriptionAsync(new PushSubscription { Endpoint = "https://fcm.googleapis.com/fcm/send/e", P256dh = "p", Auth = "a", UserID = 1 });
        await _sut.SaveSubscriptionAsync(new PushSubscription { Endpoint = "https://fcm.googleapis.com/fcm/send/e", P256dh = "p", Auth = "a", UserID = 2 });

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

    [Theory]
    [InlineData("http://push.example.com/e")]
    [InlineData("/relative/path")]
    [InlineData("not a url")]
    public async Task SaveSubscription_NonHttpsAbsoluteEndpoint_Throws(string endpoint)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SaveSubscriptionAsync(
            new PushSubscription { Endpoint = endpoint, P256dh = "p", Auth = "a", UserID = 1 }));
        _subscriptions.Verify(s => s.UpsertAsync(It.IsAny<PushSubscription>()), Times.Never);
    }

    [Fact]
    public async Task SaveSubscription_EndpointTooLong_Throws()
    {
        var endpoint = "https://fcm.googleapis.com/fcm/send/" + new string('a', 2048);
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SaveSubscriptionAsync(
            new PushSubscription { Endpoint = endpoint, P256dh = "p", Auth = "a", UserID = 1 }));
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
    [Theory]
    [InlineData("https://fcm.googleapis.com/fcm/send/abc")]
    [InlineData("https://updates.push.services.mozilla.com/wpush/v2/abc")]
    [InlineData("https://wns2-par02p.notify.windows.com/w/?token=abc")]
    [InlineData("https://web.push.apple.com/abc")]
    [InlineData("https://FCM.GoogleAPIs.com/fcm/send/abc")]
    public async Task SaveSubscription_KnownPushServiceHost_Saves(string endpoint)
    {
        await _sut.SaveSubscriptionAsync(new PushSubscription { Endpoint = endpoint, P256dh = "p", Auth = "a", UserID = 1 });
        _subscriptions.Verify(s => s.UpsertAsync(It.IsAny<PushSubscription>()), Times.Once);
    }

    [Theory]
    [InlineData("https://push.example.com/e")]
    [InlineData("https://evilfcm.googleapis.com.example.com/e")]
    [InlineData("https://notfcm.googleapis.com/e")]
    [InlineData("https://127.0.0.1/e")]
    public async Task SaveSubscription_UnknownHost_Throws(string endpoint)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SaveSubscriptionAsync(
            new PushSubscription { Endpoint = endpoint, P256dh = "p", Auth = "a", UserID = 1 }));
        _subscriptions.Verify(s => s.UpsertAsync(It.IsAny<PushSubscription>()), Times.Never);
    }

    [Fact]
    public async Task MarkSeen_WithUpTo_SetsLastSeenToUpTo()
    {
        var upTo = DateTime.UtcNow.AddMinutes(-5);
        await _sut.MarkSeenAsync(7, upTo);
        _seen.Verify(s => s.SetLastSeenAsync(7, upTo), Times.Once);
    }

    [Fact]
    public async Task MarkSeen_UpToInFuture_ClampsToNow()
    {
        DateTime? set = null;
        _seen.Setup(s => s.SetLastSeenAsync(7, It.IsAny<DateTime>()))
            .Callback<int, DateTime>((_, t) => set = t)
            .Returns(Task.CompletedTask);

        await _sut.MarkSeenAsync(7, DateTime.UtcNow.AddDays(1));

        Assert.NotNull(set);
        Assert.True(set!.Value <= DateTime.UtcNow);
    }

    [Fact]
    public async Task MarkSeen_OlderThanCurrent_DoesNotMoveBackwards()
    {
        var current = DateTime.UtcNow.AddMinutes(-1);
        _seen.Setup(s => s.GetLastSeenAsync(7)).ReturnsAsync(current);

        await _sut.MarkSeenAsync(7, current.AddMinutes(-10));

        _seen.Verify(s => s.SetLastSeenAsync(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task MarkSeen_UpToNewestShown_LaterNotificationStaysUnread()
    {
        var shown = DateTime.UtcNow.AddSeconds(-30);
        DateTime? lastSeen = null;
        _seen.Setup(s => s.SetLastSeenAsync(7, It.IsAny<DateTime>()))
            .Callback<int, DateTime>((_, t) => lastSeen = t)
            .Returns(Task.CompletedTask);
        _seen.Setup(s => s.GetLastSeenAsync(7)).ReturnsAsync(() => lastSeen);

        await _sut.MarkSeenAsync(7, shown);

        _notifications.Setup(n => n.GetLatestAsync(7, 1)).ReturnsAsync(new List<Notification> { N(shown) });
        Assert.False(await _sut.HasUnreadAsync(7));
        _notifications.Setup(n => n.GetLatestAsync(7, 1)).ReturnsAsync(new List<Notification> { N(shown.AddSeconds(10)) });
        Assert.True(await _sut.HasUnreadAsync(7));
    }

    [Fact]
    public async Task NotifyBeerAdded_SavesSharedNotificationExcludingAdder_AndQueuesIt()
    {
        Notification? saved = null;
        _notifications.Setup(n => n.SaveAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => saved = n)
            .ReturnsAsync(new Notification { ID = "returned-id" });

        await _sut.NotifyBeerAddedAsync("abc123", "Jóla Gull", "Ölgerðin", 7);

        Assert.NotNull(saved);
        Assert.Equal(NotificationTypes.Beer, saved!.Type);
        Assert.Equal("Jóla Gull · Ölgerðin", saved.Title);
        Assert.Equal("/beer/abc123", saved.Link);
        Assert.Null(saved.RecipientUserID);
        Assert.Equal(7, saved.ExcludedUserID);
        Assert.Null(saved.Key);
        Assert.True(Math.Abs((DateTime.UtcNow - saved.Created).TotalSeconds) < 5);
        _queue.Verify(q => q.EnqueueAsync("returned-id"), Times.Once);
    }

    [Fact]
    public async Task NotifyBeerAdded_TruncatesLongTitle()
    {
        Notification? saved = null;
        _notifications.Setup(n => n.SaveAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => saved = n)
            .ReturnsAsync(new Notification { ID = "id" });

        await _sut.NotifyBeerAddedAsync("abc", new string('a', 90), new string('b', 90), 7);

        Assert.Equal(NotificationInteractor.TitleMaxLength, saved!.Title.Length);
        Assert.EndsWith("…", saved.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task NotifyBeerAdded_NoBeerID_Throws(string beerID)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.NotifyBeerAddedAsync(beerID, "Jóla Gull", "Ölgerðin", 7));
        _notifications.Verify(n => n.SaveAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task NotifyBeerComment_OneNotificationPerValidDistinctRecipient()
    {
        var saved = new List<Notification>();
        _notifications.Setup(n => n.SaveAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => { lock (saved) saved.Add(n); })
            .ReturnsAsync((Notification n) => { n.ID = $"id-{n.RecipientUserID}"; return n; });

        var queued = await _sut.NotifyBeerCommentAsync("abc", "Jóla Gull", "kromby", "Góður\r\n  bjór ", new[] { 8, 9, 8, 0, -1 });

        Assert.Equal(2, queued);
        Assert.Equal(new int?[] { 8, 9 }, saved.Select(n => n.RecipientUserID).Order());
        Assert.All(saved, n =>
        {
            Assert.Equal(NotificationTypes.BeerChat, n.Type);
            Assert.Equal("kromby um Jóla Gull: Góður bjór", n.Title);
            Assert.Equal("/beer/abc", n.Link);
            Assert.Null(n.ExcludedUserID);
            Assert.Null(n.Key);
        });
        _queue.Verify(q => q.EnqueueAsync("id-8"), Times.Once);
        _queue.Verify(q => q.EnqueueAsync("id-9"), Times.Once);
    }

    [Fact]
    public async Task NotifyBeerComment_TruncatesLongTitle()
    {
        Notification? saved = null;
        _notifications.Setup(n => n.SaveAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => saved = n)
            .ReturnsAsync((Notification n) => { n.ID = "id"; return n; });

        await _sut.NotifyBeerCommentAsync("abc", "Jóla Gull", "kromby", new string('a', 500), new[] { 8 });

        Assert.Equal(NotificationInteractor.TitleMaxLength, saved!.Title.Length);
        Assert.StartsWith("kromby um Jóla Gull: aaa", saved.Title);
        Assert.EndsWith("…", saved.Title);
    }

    [Fact]
    public async Task NotifyBeerComment_OneRecipientFails_OthersStillQueued()
    {
        _notifications.Setup(n => n.SaveAsync(It.Is<Notification>(x => x.RecipientUserID == 8))).ThrowsAsync(new InvalidOperationException("storage"));
        _notifications.Setup(n => n.SaveAsync(It.Is<Notification>(x => x.RecipientUserID != 8)))
            .ReturnsAsync((Notification n) => { n.ID = $"id-{n.RecipientUserID}"; return n; });

        var queued = await _sut.NotifyBeerCommentAsync("abc", "Jóla Gull", "kromby", "Góður", new[] { 8, 9, 10 });

        Assert.Equal(2, queued);
        _queue.Verify(q => q.EnqueueAsync("id-9"), Times.Once);
        _queue.Verify(q => q.EnqueueAsync("id-10"), Times.Once);
    }

    [Fact]
    public async Task NotifyBeerComment_NoRecipients_NothingSaved()
    {
        var queued = await _sut.NotifyBeerCommentAsync("abc", "Jóla Gull", "kromby", "Góður", Array.Empty<int>());

        Assert.Equal(0, queued);
        _notifications.Verify(n => n.SaveAsync(It.IsAny<Notification>()), Times.Never);
    }
}
