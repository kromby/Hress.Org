using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.BeerRater;

public class SummaryInteractorTests
{
    private const string SeasonID = "jolabjor-2026";

    private readonly Mock<IBeerDataAccess> _beerMock;
    private readonly Mock<IBeerSeasonDataAccess> _seasonMock;
    private readonly Mock<IBeerRatingDataAccess> _ratingMock;
    private readonly Mock<IBeerChatThreadDataAccess> _threadMock;
    private readonly Mock<IBeerChatDataAccess> _chatMock;

    public SummaryInteractorTests()
    {
        _beerMock = new Mock<IBeerDataAccess>();
        _seasonMock = new Mock<IBeerSeasonDataAccess>();
        _ratingMock = new Mock<IBeerRatingDataAccess>();
        _threadMock = new Mock<IBeerChatThreadDataAccess>();
        _chatMock = new Mock<IBeerChatDataAccess>();

        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season>
        {
            new() { ID = SeasonID, Name = "Jólabjór 2026", StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc) }
        });
        _ratingMock.Setup(d => d.GetRatingsAsync(SeasonID)).ReturnsAsync(new List<Rating>());
        _threadMock.Setup(d => d.GetThreadIDsAsync(SeasonID)).ReturnsAsync(new Dictionary<string, int>());
    }

    private static Beer NewBeer(string id, string name) =>
        new() { ID = id, Name = name, Producer = "Carlsberg", Country = "Danmörk", AlcoholPercentage = 5.6 };

    private SummaryInteractor Arrange(IList<Beer> beers, IList<string> seasonBeerIDs)
    {
        _beerMock.Setup(d => d.GetBeersAsync()).ReturnsAsync(beers);
        _seasonMock.Setup(d => d.GetSeasonBeerIDsAsync(SeasonID)).ReturnsAsync(seasonBeerIDs);

        var seasonInteractor = new SeasonInteractor(_seasonMock.Object, new Mock<ILogger<SeasonInteractor>>().Object);
        var beerInteractor = new BeerInteractor(_beerMock.Object, _seasonMock.Object, seasonInteractor, new Mock<ILogger<BeerInteractor>>().Object);
        var chatInteractor = new BeerChatInteractor(_threadMock.Object, _chatMock.Object, beerInteractor, seasonInteractor, new Mock<ILogger<BeerChatInteractor>>().Object);
        return new SummaryInteractor(beerInteractor, _ratingMock.Object, chatInteractor, new Mock<ILogger<SummaryInteractor>>().Object);
    }

    [Fact]
    public async Task SeasonSummaries_OnlySeasonBeers_OrderedByName()
    {
        // ARRANGE - "Ölvisholt" sorts after "Tuborg" in Icelandic, not before "A".
        var interactor = Arrange(
            new List<Beer> { NewBeer("b1", "Tuborg Lille Jul"), NewBeer("b2", "Ölvisholt Jólabjór"), NewBeer("b3", "Egils Malt Jólabjór"), NewBeer("old", "Not this season") },
            new List<string> { "b1", "b2", "b3" });

        // ACT
        var result = await interactor.GetSeasonSummariesAsync(SeasonID, 7);

        // ASSERT
        Assert.Equal(new[] { "b3", "b1", "b2" }, result.Select(s => s.Beer.ID));
    }

    [Fact]
    public async Task SeasonSummaries_SeasonBeerMissingFromBeers_IsSkipped()
    {
        // ARRANGE
        var interactor = Arrange(new List<Beer> { NewBeer("b1", "Tuborg Lille Jul") }, new List<string> { "b1", "deleted" });

        // ACT
        var result = await interactor.GetSeasonSummariesAsync(SeasonID, 7);

        // ASSERT
        Assert.Single(result);
    }

    [Fact]
    public async Task SeasonSummaries_MyRating_OnlyFromMyRatings()
    {
        // ARRANGE
        var interactor = Arrange(
            new List<Beer> { NewBeer("b1", "Tuborg Lille Jul"), NewBeer("b2", "Jólagull") },
            new List<string> { "b1", "b2" });
        _ratingMock.Setup(d => d.GetRatingsAsync(SeasonID)).ReturnsAsync(new List<Rating>
        {
            new() { SeasonID = SeasonID, BeerID = "b1", UserID = 7, Stars = 4 },
            new() { SeasonID = SeasonID, BeerID = "b1", UserID = 8, Stars = 2 },
            new() { SeasonID = SeasonID, BeerID = "b2", UserID = 8, Stars = 5 },
        });

        // ACT
        var result = await interactor.GetSeasonSummariesAsync(SeasonID, 7);

        // ASSERT
        Assert.Equal(4, result.Single(s => s.Beer.ID == "b1").MyRating);
        Assert.Null(result.Single(s => s.Beer.ID == "b2").MyRating);
    }

    [Fact]
    public async Task SeasonSummaries_UnknownSeason_ThrowsKeyNotFound()
    {
        // ARRANGE
        var interactor = Arrange(new List<Beer>(), new List<string>());

        // ACT & ASSERT
        await Assert.ThrowsAsync<KeyNotFoundException>(() => interactor.GetSeasonSummariesAsync("nope", 7));
    }

    [Fact]
    public async Task SeasonSummaries_ClosedSeason_StillReturnsBeers()
    {
        // ARRANGE
        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season>
        {
            new() { ID = SeasonID, StartDate = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc) }
        });
        var interactor = Arrange(new List<Beer> { NewBeer("b1", "Tuborg Lille Jul") }, new List<string> { "b1" });

        // ACT
        var result = await interactor.GetSeasonSummariesAsync(SeasonID, 7);

        // ASSERT
        Assert.Single(result);
    }

    [Fact]
    public async Task SeasonSummaries_RatingCount_CountsEveryMembersRating()
    {
        // ARRANGE
        _ratingMock.Setup(d => d.GetRatingsAsync(SeasonID)).ReturnsAsync(new List<Rating>
        {
            new() { SeasonID = SeasonID, BeerID = "b1", UserID = 7, Stars = 4 },
            new() { SeasonID = SeasonID, BeerID = "b1", UserID = 8, Stars = 2 },
            new() { SeasonID = SeasonID, BeerID = "b2", UserID = 8, Stars = 5 },
        });
        var interactor = Arrange(
            new List<Beer> { NewBeer("b1", "Tuborg Lille Jul"), NewBeer("b2", "Jóla Kaldi"), NewBeer("b3", "Egils Malt Jólabjór") },
            new List<string> { "b1", "b2", "b3" });

        // ACT
        var result = await interactor.GetSeasonSummariesAsync(SeasonID, 7);

        // ASSERT
        Assert.Equal(2, result.Single(s => s.Beer.ID == "b1").RatingCount);
        Assert.Equal(1, result.Single(s => s.Beer.ID == "b2").RatingCount);
        Assert.Equal(0, result.Single(s => s.Beer.ID == "b3").RatingCount);
    }

    [Fact]
    public async Task SeasonSummaries_MessageCount_FromEachBeersChatThread()
    {
        // ARRANGE - b3 has a thread with no messages left.
        _threadMock.Setup(d => d.GetThreadIDsAsync(SeasonID)).ReturnsAsync(new Dictionary<string, int> { ["b1"] = 101, ["b3"] = 103 });
        _chatMock.Setup(d => d.GetMessageCountsAsync(It.IsAny<IReadOnlyCollection<int>>())).ReturnsAsync(new Dictionary<int, int> { [101] = 3 });
        var interactor = Arrange(
            new List<Beer> { NewBeer("b1", "Tuborg Lille Jul"), NewBeer("b2", "Jóla Kaldi"), NewBeer("b3", "Egils Malt Jólabjór") },
            new List<string> { "b1", "b2", "b3" });

        // ACT
        var result = await interactor.GetSeasonSummariesAsync(SeasonID, 7);

        // ASSERT
        Assert.Equal(3, result.Single(s => s.Beer.ID == "b1").MessageCount);
        Assert.Equal(0, result.Single(s => s.Beer.ID == "b2").MessageCount);
        Assert.Equal(0, result.Single(s => s.Beer.ID == "b3").MessageCount);
        _chatMock.Verify(d => d.GetMessageCountsAsync(It.Is<IReadOnlyCollection<int>>(ids => ids.OrderBy(i => i).SequenceEqual(new[] { 101, 103 }))), Times.Once);
    }

    [Fact]
    public async Task SeasonSummaries_NoChatThreads_DoesNotQueryTheChat()
    {
        // ARRANGE
        var interactor = Arrange(new List<Beer> { NewBeer("b1", "Tuborg Lille Jul") }, new List<string> { "b1" });

        // ACT
        var result = await interactor.GetSeasonSummariesAsync(SeasonID, 7);

        // ASSERT
        Assert.Equal(0, result.Single().MessageCount);
        _chatMock.Verify(d => d.GetMessageCountsAsync(It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
    }

    [Fact]
    public async Task SeasonSummaries_ChatUnavailable_StillReturnsBeersWithRatingCounts()
    {
        // ARRANGE
        _ratingMock.Setup(d => d.GetRatingsAsync(SeasonID)).ReturnsAsync(new List<Rating>
        {
            new() { SeasonID = SeasonID, BeerID = "b1", UserID = 7, Stars = 4 },
        });
        _threadMock.Setup(d => d.GetThreadIDsAsync(SeasonID)).ReturnsAsync(new Dictionary<string, int> { ["b1"] = 101 });
        _chatMock.Setup(d => d.GetMessageCountsAsync(It.IsAny<IReadOnlyCollection<int>>())).ThrowsAsync(new InvalidOperationException("SQL is down"));
        var interactor = Arrange(new List<Beer> { NewBeer("b1", "Tuborg Lille Jul") }, new List<string> { "b1" });

        // ACT
        var result = await interactor.GetSeasonSummariesAsync(SeasonID, 7);

        // ASSERT
        var summary = Assert.Single(result);
        Assert.Equal(1, summary.RatingCount);
        Assert.Equal(0, summary.MessageCount);
    }
}
