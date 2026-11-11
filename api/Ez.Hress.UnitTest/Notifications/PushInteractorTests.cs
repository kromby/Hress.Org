using Ez.Hress.Notifications.DataAccess;
using Ez.Hress.Notifications.Entities;
using Ez.Hress.Notifications.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Notifications;

public class PushInteractorTests
{
    private readonly Mock<INotificationDataAccess> _notifications = new();
    private readonly Mock<IPushSubscriptionDataAccess> _subscriptions = new();
    private readonly Mock<IPushSender> _sender = new();
    private readonly PushInteractor _sut;
    private readonly Notification _notification = new() { ID = "n1", Type = "news", Title = "Bjórkvöld", Link = "/news/42" };

    public PushInteractorTests()
    {
        _notifications.Setup(n => n.GetAsync("n1")).ReturnsAsync(_notification);
        _sender.Setup(s => s.SendAsync(It.IsAny<PushSubscription>(), It.IsAny<PushPayload>())).ReturnsAsync(PushSendResult.Sent);
        _sut = new PushInteractor(_notifications.Object, _subscriptions.Object, _sender.Object, Mock.Of<ILogger<PushInteractor>>());
    }

    private static PushSubscription Sub(string endpoint, int userID = 1) => new() { Endpoint = endpoint, UserID = userID };

    private void SetSubs(params PushSubscription[] subs) =>
        _subscriptions.Setup(s => s.GetAllAsync()).ReturnsAsync(subs.ToList());

    [Fact]
    public async Task Send_NewsNotification_SendsTitleBodyLinkToAll()
    {
        SetSubs(Sub("a"), Sub("b"), Sub("c"));

        var result = await _sut.SendAsync("n1");

        var expected = new PushPayload("Ný frétt", "Bjórkvöld", "/news/42");
        _sender.Verify(s => s.SendAsync(It.IsAny<PushSubscription>(), expected), Times.Exactly(3));
        Assert.Equal(new PushRunResult(3, 0, 0), result);
    }

    [Fact]
    public async Task Send_GoneSubscriptionIsDeleted_OthersStillSent()
    {
        SetSubs(Sub("a"), Sub("b"), Sub("c"), Sub("d"));
        _sender.Setup(s => s.SendAsync(It.Is<PushSubscription>(x => x.Endpoint == "b"), It.IsAny<PushPayload>())).ReturnsAsync(PushSendResult.Gone);
        _sender.Setup(s => s.SendAsync(It.Is<PushSubscription>(x => x.Endpoint == "c"), It.IsAny<PushPayload>())).ReturnsAsync(PushSendResult.Failed);

        var result = await _sut.SendAsync("n1");

        _subscriptions.Verify(s => s.DeleteAsync("b"), Times.Once);
        _subscriptions.Verify(s => s.DeleteAsync(It.Is<string>(e => e != "b")), Times.Never);
        Assert.Equal(new PushRunResult(2, 1, 1), result);
    }

    [Fact]
    public async Task Send_GoneAndDeleteThrows_OthersStillSent()
    {
        SetSubs(Sub("a"), Sub("b"), Sub("c"));
        _sender.Setup(s => s.SendAsync(It.Is<PushSubscription>(x => x.Endpoint == "a"), It.IsAny<PushPayload>())).ReturnsAsync(PushSendResult.Gone);
        _subscriptions.Setup(s => s.DeleteAsync("a")).ThrowsAsync(new InvalidOperationException("storage"));

        var result = await _sut.SendAsync("n1");

        _sender.Verify(s => s.SendAsync(It.IsAny<PushSubscription>(), It.IsAny<PushPayload>()), Times.Exactly(3));
        Assert.Equal(new PushRunResult(2, 0, 1), result);
    }

    [Fact]
    public void Payload_SerializesWithTitleBodyLinkKeys()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new PushPayload("Ný frétt", "t", "/news/1"), WebPushSender.JsonOptions);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var keys = doc.RootElement.EnumerateObject().Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "title", "body", "link" }, keys);
    }

    [Fact]
    public async Task Send_SenderThrows_CountsAsFailedAndContinues()
    {
        SetSubs(Sub("a"), Sub("b"), Sub("c"));
        _sender.Setup(s => s.SendAsync(It.Is<PushSubscription>(x => x.Endpoint == "b"), It.IsAny<PushPayload>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var result = await _sut.SendAsync("n1");

        _sender.Verify(s => s.SendAsync(It.IsAny<PushSubscription>(), It.IsAny<PushPayload>()), Times.Exactly(3));
        Assert.Equal(new PushRunResult(2, 0, 1), result);
    }

    [Fact]
    public async Task Send_RecipientSet_OnlyRecipientsDevices()
    {
        _notification.RecipientUserID = 5;
        SetSubs(Sub("a", 5), Sub("b", 6), Sub("c", 5));

        var result = await _sut.SendAsync("n1");

        _sender.Verify(s => s.SendAsync(It.Is<PushSubscription>(x => x.UserID == 5), It.IsAny<PushPayload>()), Times.Exactly(2));
        _sender.Verify(s => s.SendAsync(It.Is<PushSubscription>(x => x.UserID != 5), It.IsAny<PushPayload>()), Times.Never);
        Assert.Equal(new PushRunResult(2, 0, 0), result);
    }

    [Fact]
    public async Task Send_NotificationMissing_ReturnsZeroWithoutThrowing()
    {
        _notifications.Setup(n => n.GetAsync("missing")).ReturnsAsync((Notification?)null);

        var result = await _sut.SendAsync("missing");

        Assert.Equal(new PushRunResult(0, 0, 0), result);
        _sender.Verify(s => s.SendAsync(It.IsAny<PushSubscription>(), It.IsAny<PushPayload>()), Times.Never);
    }

    [Fact]
    public async Task Send_StorageFailure_Throws()
    {
        _subscriptions.Setup(s => s.GetAllAsync()).ThrowsAsync(new InvalidOperationException("storage down"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SendAsync("n1"));
    }
}
