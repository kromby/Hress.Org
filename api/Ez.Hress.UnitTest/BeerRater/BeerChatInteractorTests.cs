using Ez.Hress.BeerRater.DataAccess;
using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Chat.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using HressChat = Ez.Hress.Chat.Entities;

namespace Ez.Hress.UnitTest.BeerRater;

public class BeerChatInteractorTests
{
    private const string SeasonID = "jolabjor-2026";
    private const string BeerID = "b1";
    private const int ThreadID = 1234;

    private readonly Mock<IBeerDataAccess> _beerMock;
    private readonly Mock<IBeerSeasonDataAccess> _seasonMock;
    private readonly Mock<IBeerChatThreadDataAccess> _threadMock;
    private readonly Mock<IBeerChatDataAccess> _chatMock;

    public BeerChatInteractorTests()
    {
        _beerMock = new Mock<IBeerDataAccess>();
        _seasonMock = new Mock<IBeerSeasonDataAccess>();
        _threadMock = new Mock<IBeerChatThreadDataAccess>();
        _chatMock = new Mock<IBeerChatDataAccess>();

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

    private BeerChatInteractor Arrange()
    {
        var seasonInteractor = new SeasonInteractor(_seasonMock.Object, new Mock<ILogger<SeasonInteractor>>().Object);
        var beerInteractor = new BeerInteractor(_beerMock.Object, _seasonMock.Object, seasonInteractor, new Mock<ILogger<BeerInteractor>>().Object);
        return new BeerChatInteractor(_threadMock.Object, _chatMock.Object, beerInteractor, new Mock<ILogger<BeerChatInteractor>>().Object);
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
        var access = new HressChatAccess(chatInteractor, new Mock<ILogger<HressChatAccess>>().Object);

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
}
