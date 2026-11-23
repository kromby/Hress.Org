using Ez.Hress.BeerRater.DataAccess;
using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Chat.UseCases;
using Ez.Hress.Notifications.Entities;
using Ez.Hress.Notifications.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using HressChat = Ez.Hress.Chat.Entities;

namespace Ez.Hress.UnitTest.BeerRater;

public class BeerChatInteractorTests
{
    private const string SeasonID = "jolabjor-2026";
    private const string BeerID = "b1";
    private const int ThreadID = 1234;
    private const int CategoryID = 77;
    private const int UserID = 7;

    private readonly Mock<IBeerDataAccess> _beerMock;
    private readonly Mock<IBeerSeasonDataAccess> _seasonMock;
    private readonly Mock<IBeerChatThreadDataAccess> _threadMock;
    private readonly Mock<IBeerChatDataAccess> _chatMock;
    private readonly Mock<IBeerRatingDataAccess> _ratingMock = new();
    private readonly Mock<INotificationDataAccess> _notifications = new();
    private readonly Mock<IPushQueue> _queue = new();
    private readonly List<Notification> _saved = new();

    public BeerChatInteractorTests()
    {
        _beerMock = new Mock<IBeerDataAccess>();
        _seasonMock = new Mock<IBeerSeasonDataAccess>();
        _threadMock = new Mock<IBeerChatThreadDataAccess>();
        _chatMock = new Mock<IBeerChatDataAccess>();

        // Nobody else is active unless a test says so.
        _ratingMock.Setup(d => d.GetRatingsAsync(It.IsAny<string>())).ReturnsAsync(new List<Rating>());
        _chatMock.Setup(d => d.GetPosterIDsAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<int>());
        _notifications.Setup(n => n.SaveAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => { lock (_saved) _saved.Add(n); })
            .ReturnsAsync((Notification n) => { n.ID = $"id-{n.RecipientUserID}"; return n; });

        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season>
        {
            new() { ID = SeasonID, Name = "Jólabjór 2026", StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc) }
        });
        _seasonMock.Setup(d => d.GetSeasonBeerIDsAsync(SeasonID)).ReturnsAsync(new List<string> { BeerID });
        _beerMock.Setup(d => d.GetBeersAsync()).ReturnsAsync(new List<Beer>
        {
            new() { ID = BeerID, Name = "Tuborg Lille Jul" },
            new() { ID = "old", Name = "Not this season" }
        });
    }

    private BeerChatInteractor Arrange(TimeSpan? notifyTimeout = null)
    {
        var seasonInteractor = new SeasonInteractor(_seasonMock.Object, new Mock<ILogger<SeasonInteractor>>().Object);
        var beerInteractor = new BeerInteractor(_beerMock.Object, _seasonMock.Object, seasonInteractor, new Mock<ILogger<BeerInteractor>>().Object);
        var activityInteractor = new BeerActivityInteractor(_beerMock.Object, _seasonMock.Object, _ratingMock.Object, _chatMock.Object, new Mock<ILogger<BeerActivityInteractor>>().Object);
        var notificationInteractor = new NotificationInteractor(_notifications.Object, Mock.Of<INotificationSeenDataAccess>(),
            Mock.Of<IPushSubscriptionDataAccess>(), _queue.Object, Mock.Of<ILogger<NotificationInteractor>>());
        return new BeerChatInteractor(_threadMock.Object, _chatMock.Object, beerInteractor, seasonInteractor, activityInteractor, notificationInteractor,
            new Mock<ILogger<BeerChatInteractor>>().Object, notifyTimeout);
    }

    // Users 7 (the commenter), 8 and 9 posted recently, and user 10 rated a beer recently.
    private void ArrangeActiveMembers()
    {
        _threadMock.Setup(d => d.GetThreadIDAsync(SeasonID, BeerID)).ReturnsAsync(ThreadID);
        _chatMock.Setup(d => d.GetPosterIDsAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<int> { UserID, 8, 9 });
        _ratingMock.Setup(d => d.GetRatingsAsync(SeasonID)).ReturnsAsync(new List<Rating>
        {
            new() { SeasonID = SeasonID, BeerID = BeerID, UserID = 10, Stars = 4, Created = DateTime.UtcNow.AddDays(-1), Updated = DateTime.UtcNow.AddDays(-1) }
        });
        _chatMock.Setup(d => d.PostMessageAsync(ThreadID, UserID, It.IsAny<string>()))
            .ReturnsAsync((int _, int _, string text) => new ChatMessage { ID = 9, UserID = UserID, Username = "kromby", Text = text });
    }

    [Fact]
    public async Task GetMessages_NoThread_ReturnsEmpty()
    {
        // ARRANGE
        _threadMock.Setup(d => d.GetThreadIDAsync(SeasonID, BeerID)).ReturnsAsync((int?)null);
        var interactor = Arrange();

        // ACT
        var result = await interactor.GetMessagesAsync(SeasonID, BeerID);

        // ASSERT
        Assert.Empty(result);
        _chatMock.Verify(d => d.GetMessagesAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetMessages_Thread_ReturnsThreadMessages()
    {
        // ARRANGE
        _threadMock.Setup(d => d.GetThreadIDAsync(SeasonID, BeerID)).ReturnsAsync(ThreadID);
        _chatMock.Setup(d => d.GetMessagesAsync(ThreadID)).ReturnsAsync(new List<ChatMessage>
        {
            new() { ID = 1, UserID = 7, Text = "Góður" },
            new() { ID = 2, UserID = 8, Text = "Of sætur" },
        });
        var interactor = Arrange();

        // ACT
        var result = await interactor.GetMessagesAsync(SeasonID, BeerID);

        // ASSERT
        Assert.Equal(new[] { 1, 2 }, result.Select(m => m.ID));
    }

    [Fact]
    public async Task GetMessages_ClosedSeason_StillReturnsMessages()
    {
        // ARRANGE
        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season>
        {
            new() { ID = SeasonID, StartDate = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc) }
        });
        _threadMock.Setup(d => d.GetThreadIDAsync(SeasonID, BeerID)).ReturnsAsync(ThreadID);
        _chatMock.Setup(d => d.GetMessagesAsync(ThreadID)).ReturnsAsync(new List<ChatMessage> { new() { ID = 1 } });
        var interactor = Arrange();

        // ACT
        var result = await interactor.GetMessagesAsync(SeasonID, BeerID);

        // ASSERT
        Assert.Single(result);
    }

    [Fact]
    public async Task GetMessages_BeerNotInSeason_ThrowsKeyNotFound()
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAsync<KeyNotFoundException>(() => interactor.GetMessagesAsync(SeasonID, "old"));
        _threadMock.Verify(d => d.GetThreadIDAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetMessages_UnknownSeason_ThrowsKeyNotFound()
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAsync<KeyNotFoundException>(() => interactor.GetMessagesAsync("nope", BeerID));
    }

    [Fact]
    public async Task HressChatAccess_MapsChatMessages()
    {
        // ARRANGE
        var inserted = new DateTime(2026, 11, 4, 20, 15, 0, DateTimeKind.Utc);
        var updated = inserted.AddMinutes(5);
        var chatData = new Mock<IChatDataAccess>();
        chatData.Setup(d => d.GetMessagesAsync(ThreadID)).ReturnsAsync(new List<HressChat.ChatMessage>
        {
            new() { ID = 5, ThreadID = ThreadID, UserID = 7, Username = "kromby", AuthorImageID = 55, Text = "Góður<br />bjór", Inserted = inserted, Updated = updated },
        });
        var chatInteractor = new ChatInteractor(chatData.Object, new Mock<ILogger<ChatInteractor>>().Object);
        var access = new HressChatAccess(chatInteractor, CategoryID, new Mock<ILogger<HressChatAccess>>().Object);

        // ACT
        var result = await access.GetMessagesAsync(ThreadID);

        // ASSERT
        var message = Assert.Single(result);
        Assert.Equal(5, message.ID);
        Assert.Equal(7, message.UserID);
        Assert.Equal("kromby", message.Username);
        Assert.Equal(55, message.AuthorImageID);
        Assert.Equal("Góður\nbjór", message.Text);
        Assert.Equal(inserted, message.Created);
        Assert.Equal(updated, message.Edited);
    }

    [Fact]
    public async Task Post_ExistingThread_PostsTrimmedText()
    {
        // ARRANGE
        _threadMock.Setup(d => d.GetThreadIDAsync(SeasonID, BeerID)).ReturnsAsync(ThreadID);
        _chatMock.Setup(d => d.PostMessageAsync(ThreadID, UserID, "Góður")).ReturnsAsync(new ChatMessage { ID = 9, UserID = UserID, Text = "Góður" });
        var interactor = Arrange();

        // ACT
        var result = await interactor.PostMessageAsync(SeasonID, BeerID, UserID, "  Góður \n");

        // ASSERT
        Assert.Equal(9, result.ID);
        _chatMock.Verify(d => d.CreateThreadAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Post_FirstMessage_CreatesAndSavesThread()
    {
        // ARRANGE
        _threadMock.Setup(d => d.GetThreadIDAsync(SeasonID, BeerID)).ReturnsAsync((int?)null);
        _chatMock.Setup(d => d.CreateThreadAsync("Tuborg Lille Jul (Jólabjór 2026)", UserID)).ReturnsAsync(ThreadID);
        _threadMock.Setup(d => d.TryAddThreadAsync(SeasonID, BeerID, ThreadID)).ReturnsAsync(true);
        _chatMock.Setup(d => d.PostMessageAsync(ThreadID, UserID, "Fyrstur!")).ReturnsAsync(new ChatMessage { ID = 1 });
        var interactor = Arrange();

        // ACT
        await interactor.PostMessageAsync(SeasonID, BeerID, UserID, "Fyrstur!");

        // ASSERT
        _threadMock.Verify(d => d.TryAddThreadAsync(SeasonID, BeerID, ThreadID), Times.Once);
        _chatMock.Verify(d => d.PostMessageAsync(ThreadID, UserID, "Fyrstur!"), Times.Once);
        _chatMock.Verify(d => d.DeleteThreadAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Post_FirstMessageRace_DeletesOwnThread_UsesWinners()
    {
        // ARRANGE - another member saved thread 1234 between our lookup and our save.
        const int ownThreadID = 5678;
        _threadMock.SetupSequence(d => d.GetThreadIDAsync(SeasonID, BeerID))
            .ReturnsAsync((int?)null)
            .ReturnsAsync(ThreadID);
        _chatMock.Setup(d => d.CreateThreadAsync(It.IsAny<string>(), UserID)).ReturnsAsync(ownThreadID);
        _threadMock.Setup(d => d.TryAddThreadAsync(SeasonID, BeerID, ownThreadID)).ReturnsAsync(false);
        _chatMock.Setup(d => d.PostMessageAsync(ThreadID, UserID, "Ég líka")).ReturnsAsync(new ChatMessage { ID = 2 });
        var interactor = Arrange();

        // ACT
        await interactor.PostMessageAsync(SeasonID, BeerID, UserID, "Ég líka");

        // ASSERT
        _chatMock.Verify(d => d.DeleteThreadAsync(ownThreadID, UserID), Times.Once);
        _chatMock.Verify(d => d.PostMessageAsync(ThreadID, UserID, "Ég líka"), Times.Once);
        _chatMock.Verify(d => d.PostMessageAsync(ownThreadID, It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   \n ")]
    public async Task Post_BlankText_ThrowsArgument_CreatesNothing(string? text)
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAnyAsync<ArgumentException>(() => interactor.PostMessageAsync(SeasonID, BeerID, UserID, text));
        _chatMock.Verify(d => d.CreateThreadAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _chatMock.Verify(d => d.PostMessageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Post_TextOver2000_ThrowsArgument()
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAnyAsync<ArgumentException>(() => interactor.PostMessageAsync(SeasonID, BeerID, UserID, new string('a', 2001)));
        _chatMock.Verify(d => d.PostMessageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Post_ClosedSeason_ThrowsSeasonClosed()
    {
        // ARRANGE
        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season>
        {
            new() { ID = SeasonID, StartDate = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc) }
        });
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAsync<SeasonClosedException>(() => interactor.PostMessageAsync(SeasonID, BeerID, UserID, "Of seint"));
        _chatMock.Verify(d => d.CreateThreadAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _chatMock.Verify(d => d.PostMessageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Post_BeerNotInSeason_ThrowsKeyNotFound()
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAsync<KeyNotFoundException>(() => interactor.PostMessageAsync(SeasonID, "old", UserID, "Hvar?"));
        _chatMock.Verify(d => d.CreateThreadAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HressChatAccess_NoCategorySetting_ThrowsInvalidOperation()
    {
        // ARRANGE
        var chatData = new Mock<IChatDataAccess>();
        var chatInteractor = new ChatInteractor(chatData.Object, new Mock<ILogger<ChatInteractor>>().Object);
        var access = new HressChatAccess(chatInteractor, 0, new Mock<ILogger<HressChatAccess>>().Object);

        // ACT & ASSERT - a configuration error (500), not a bad request (400).
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => access.CreateThreadAsync("Tuborg Lille Jul (Jólabjór 2026)", UserID));
        Assert.IsNotType<SeasonClosedException>(ex);
        chatData.Verify(d => d.CreateThreadAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task HressChatAccess_UnknownCategory_ThrowsInvalidOperation()
    {
        // ARRANGE
        var chatData = new Mock<IChatDataAccess>();
        chatData.Setup(d => d.CreateThreadAsync(CategoryID, It.IsAny<string>(), UserID, It.IsAny<DateTime>())).ThrowsAsync(new KeyNotFoundException());
        var chatInteractor = new ChatInteractor(chatData.Object, new Mock<ILogger<ChatInteractor>>().Object);
        var access = new HressChatAccess(chatInteractor, CategoryID, new Mock<ILogger<HressChatAccess>>().Object);

        // ACT & ASSERT - not a 404, since the beer exists.
        await Assert.ThrowsAsync<InvalidOperationException>(() => access.CreateThreadAsync("Tuborg Lille Jul (Jólabjór 2026)", UserID));
    }

    [Fact]
    public async Task Post_NotifiesActiveMembersExceptCommenter()
    {
        // ARRANGE
        ArrangeActiveMembers();
        var interactor = Arrange();

        // ACT
        await interactor.PostMessageAsync(SeasonID, BeerID, UserID, "Góður\n\nbjór");

        // ASSERT
        Assert.Equal(new int?[] { 8, 9, 10 }, _saved.Select(n => n.RecipientUserID).Order());
        Assert.All(_saved, n =>
        {
            Assert.Equal(NotificationTypes.BeerChat, n.Type);
            Assert.Equal("kromby um Tuborg Lille Jul: Góður bjór", n.Title);
            Assert.Equal($"/beer/{BeerID}", n.Link);
            Assert.Null(n.ExcludedUserID);
        });
        _queue.Verify(q => q.EnqueueAsync(It.IsAny<string>()), Times.Exactly(3));
        _queue.Verify(q => q.EnqueueAsync($"id-{UserID}"), Times.Never);
    }

    [Fact]
    public async Task Post_LongComment_TitleTruncated()
    {
        // ARRANGE
        ArrangeActiveMembers();
        var interactor = Arrange();

        // ACT
        await interactor.PostMessageAsync(SeasonID, BeerID, UserID, new string('a', 500));

        // ASSERT
        Assert.All(_saved, n =>
        {
            Assert.Equal(NotificationInteractor.TitleMaxLength, n.Title.Length);
            Assert.StartsWith("kromby um Tuborg Lille Jul: aaa", n.Title);
            Assert.EndsWith("…", n.Title);
        });
    }

    [Fact]
    public async Task Post_OnlyCommenterActive_NothingSaved()
    {
        // ARRANGE
        _threadMock.Setup(d => d.GetThreadIDAsync(SeasonID, BeerID)).ReturnsAsync(ThreadID);
        _chatMock.Setup(d => d.GetPosterIDsAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<int> { UserID });
        _chatMock.Setup(d => d.PostMessageAsync(ThreadID, UserID, "Góður")).ReturnsAsync(new ChatMessage { ID = 9, UserID = UserID, Username = "kromby", Text = "Góður" });
        var interactor = Arrange();

        // ACT
        await interactor.PostMessageAsync(SeasonID, BeerID, UserID, "Góður");

        // ASSERT
        _notifications.Verify(n => n.SaveAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task Post_ActivityLookupFails_StillReturnsMessage()
    {
        // ARRANGE
        ArrangeActiveMembers();
        _ratingMock.Setup(d => d.GetRatingsAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("storage down"));
        var interactor = Arrange();

        // ACT
        var result = await interactor.PostMessageAsync(SeasonID, BeerID, UserID, "Góður");

        // ASSERT
        Assert.Equal(9, result.ID);
        _notifications.Verify(n => n.SaveAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task Post_SavingNotificationFails_StillReturnsMessage()
    {
        // ARRANGE
        ArrangeActiveMembers();
        _notifications.Setup(n => n.SaveAsync(It.IsAny<Notification>())).ThrowsAsync(new InvalidOperationException("storage down"));
        var interactor = Arrange();

        // ACT
        var result = await interactor.PostMessageAsync(SeasonID, BeerID, UserID, "Góður");

        // ASSERT
        Assert.Equal(9, result.ID);
    }

    [Fact]
    public async Task Post_NotifierHangs_StillReturnsWithinTimeout()
    {
        // ARRANGE
        ArrangeActiveMembers();
        _notifications.Setup(n => n.SaveAsync(It.IsAny<Notification>())).Returns(new TaskCompletionSource<Notification>().Task);
        var interactor = Arrange(TimeSpan.FromMilliseconds(50));

        // ACT
        var task = interactor.PostMessageAsync(SeasonID, BeerID, UserID, "Góður");
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(3)));

        // ASSERT
        Assert.Same(task, completed);
        Assert.Equal(9, (await task).ID);
    }

    [Fact]
    public async Task HressChatAccess_GetPosterIDs_UsesCategory()
    {
        // ARRANGE
        var since = new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc);
        var chatData = new Mock<IChatDataAccess>();
        chatData.Setup(d => d.GetPosterIDsAsync(CategoryID, since)).ReturnsAsync(new List<int> { 7, 8 });
        var chatInteractor = new ChatInteractor(chatData.Object, new Mock<ILogger<ChatInteractor>>().Object);
        var access = new HressChatAccess(chatInteractor, CategoryID, new Mock<ILogger<HressChatAccess>>().Object);

        // ACT
        var result = await access.GetPosterIDsAsync(since);

        // ASSERT
        Assert.Equal(new[] { 7, 8 }, result);
    }

    [Fact]
    public async Task HressChatAccess_GetPosterIDs_NoCategorySetting_ThrowsInvalidOperation()
    {
        // ARRANGE
        var chatData = new Mock<IChatDataAccess>();
        var chatInteractor = new ChatInteractor(chatData.Object, new Mock<ILogger<ChatInteractor>>().Object);
        var access = new HressChatAccess(chatInteractor, 0, new Mock<ILogger<HressChatAccess>>().Object);

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => access.GetPosterIDsAsync(DateTime.UtcNow));
        chatData.Verify(d => d.GetPosterIDsAsync(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }
}
