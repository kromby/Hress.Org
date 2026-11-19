using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Notifications.Entities;
using Ez.Hress.Notifications.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Hardhead;

public class HardheadReminderInteractorTests
{
    private readonly Mock<IHardheadDataAccess> _hardheads = new();
    private readonly Mock<INotificationDataAccess> _notifications = new();
    private readonly Mock<IPushQueue> _queue = new();
    private readonly List<Notification> _saved = new();
    private readonly HardheadReminderInteractor _sut;

    private static readonly DateTime Today = new(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc);

    public HardheadReminderInteractorTests()
    {
        _notifications.Setup(n => n.SaveIfNewAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => { _saved.Add(n); n.ID = $"{n.RecipientUserID}|{n.Key}"; return n; });
        _hardheads.Setup(h => h.GetAttendeeIDs(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<int> { 7, 8 });

        var notificationInteractor = new NotificationInteractor(_notifications.Object, Mock.Of<INotificationSeenDataAccess>(),
            Mock.Of<IPushSubscriptionDataAccess>(), _queue.Object, Mock.Of<ILogger<NotificationInteractor>>());
        _sut = new HardheadReminderInteractor(_hardheads.Object, notificationInteractor, Mock.Of<ILogger<HardheadReminderInteractor>>());
    }

    private static HardheadNight Night(int id, DateTime date, DateTime? updated = null, string? location = null) =>
        new(id, 250, new UserBasicEntity { ID = 3, Username = "Jónsi" }) { Date = date, Updated = updated, Location = location };

    private void SetNights(params HardheadNight[] nights) =>
        _hardheads.Setup(h => h.GetHardheadsOnDay(Today.Date)).ReturnsAsync(nights.ToList());

    [Fact]
    public async Task NotifyTonight_NightToday_NotifiesRecentAttendees()
    {
        SetNights(Night(123, Today.Date));

        var queued = await _sut.NotifyTonightAsync(Today);

        Assert.Equal(2, queued);
        Assert.Equal(new int?[] { 7, 8 }, _saved.Select(n => n.RecipientUserID));
        Assert.All(_saved, n => Assert.Equal("/hardhead/123", n.Link));
        Assert.All(_saved, n => Assert.Equal("Harðhaus #250 er í kvöld hjá Jónsi", n.Title));
    }

    [Fact]
    public async Task NotifyTonight_IncludesStartTimeAndLocation()
    {
        SetNights(Night(123, Today.Date.AddHours(20), location: "Bláskógar 3"));

        await _sut.NotifyTonightAsync(Today);

        Assert.All(_saved, n => Assert.Equal("Harðhaus #250 er í kvöld kl. 20:00 hjá Jónsi · Bláskógar 3", n.Title));
    }

    [Fact]
    public async Task NotifyTonight_AttendeesFromTheSixMonthsBeforeToday()
    {
        SetNights(Night(123, Today.Date));

        await _sut.NotifyTonightAsync(Today);

        _hardheads.Verify(h => h.GetAttendeeIDs(new DateTime(2026, 5, 20), new DateTime(2026, 11, 19, 23, 59, 59)), Times.Once);
    }

    [Fact]
    public async Task NotifyTonight_NoNight_NothingSentAndNoAttendeeLookup()
    {
        SetNights();

        var queued = await _sut.NotifyTonightAsync(Today);

        Assert.Equal(0, queued);
        _hardheads.Verify(h => h.GetAttendeeIDs(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        _queue.Verify(q => q.EnqueueAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task NotifyTonight_PlaceholderDate_Skipped()
    {
        var lastDay = new DateTime(2026, 11, 30, 9, 0, 0, DateTimeKind.Utc);
        _hardheads.Setup(h => h.GetHardheadsOnDay(lastDay.Date)).ReturnsAsync(new List<HardheadNight> { Night(123, lastDay.Date) });

        var queued = await _sut.NotifyTonightAsync(lastDay);

        Assert.Equal(0, queued);
        Assert.Empty(_saved);
    }

    [Fact]
    public async Task NotifyTonight_LastDayOfMonthSetByHost_Notifies()
    {
        var lastDay = new DateTime(2026, 11, 30, 9, 0, 0, DateTimeKind.Utc);
        _hardheads.Setup(h => h.GetHardheadsOnDay(lastDay.Date))
            .ReturnsAsync(new List<HardheadNight> { Night(123, lastDay.Date, updated: new DateTime(2026, 11, 2)) });

        var queued = await _sut.NotifyTonightAsync(lastDay);

        Assert.Equal(2, queued);
    }

    [Fact]
    public async Task NotifyTonight_NightOnAnotherDay_Ignored()
    {
        SetNights(Night(123, Today.Date.AddDays(1)));

        var queued = await _sut.NotifyTonightAsync(Today);

        Assert.Equal(0, queued);
    }

    [Fact]
    public async Task NotifyTonight_TwoNights_EachNotified()
    {
        SetNights(Night(123, Today.Date), Night(124, Today.Date.AddHours(20)));

        var queued = await _sut.NotifyTonightAsync(Today);

        Assert.Equal(4, queued);
        Assert.Equal(2, _saved.Count(n => n.Link == "/hardhead/123"));
        Assert.Equal(2, _saved.Count(n => n.Link == "/hardhead/124"));
    }

    [Theory]
    [InlineData(2026, 11, 30, false, true)]
    [InlineData(2026, 11, 30, true, false)]
    [InlineData(2026, 11, 29, false, false)]
    [InlineData(2028, 2, 29, false, true)]
    [InlineData(2027, 2, 28, false, true)]
    public void IsPlaceholderDate(int year, int month, int day, bool edited, bool expected)
    {
        var night = Night(1, new DateTime(year, month, day), edited ? new DateTime(year, month, 1) : null);

        Assert.Equal(expected, HardheadReminderInteractor.IsPlaceholderDate(night));
    }
}
