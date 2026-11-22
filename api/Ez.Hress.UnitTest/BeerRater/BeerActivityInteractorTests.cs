using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.BeerRater;

public class BeerActivityInteractorTests
{
    private static readonly DateTime Since = new(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Recent = Since.AddDays(10);
    private static readonly DateTime Old = Since.AddDays(-1);

    private readonly Mock<IBeerDataAccess> _beerMock = new();
    private readonly Mock<IBeerSeasonDataAccess> _seasonMock = new();
    private readonly Mock<IBeerRatingDataAccess> _ratingMock = new();
    private readonly Mock<IBeerChatDataAccess> _chatMock = new();

    public BeerActivityInteractorTests()
    {
        _beerMock.Setup(d => d.GetBeersAsync()).ReturnsAsync(new List<Beer>());
        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season>());
        _ratingMock.Setup(d => d.GetRatingsAsync(It.IsAny<string>())).ReturnsAsync(new List<Rating>());
        _chatMock.Setup(d => d.GetPosterIDsAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<int>());
    }

    private BeerActivityInteractor Arrange() =>
        new(_beerMock.Object, _seasonMock.Object, _ratingMock.Object, _chatMock.Object, new Mock<ILogger<BeerActivityInteractor>>().Object);

    private static Rating Rated(int userID, DateTime created, DateTime updated) =>
        new() { BeerID = "b1", UserID = userID, Stars = 3, Created = created, Updated = updated };

    [Fact]
    public void ActiveSince_IsSixMonthsBack()
    {
        Assert.Equal(new DateTime(2026, 5, 22, 8, 0, 0, DateTimeKind.Utc), BeerActivityInteractor.ActiveSince(new DateTime(2026, 11, 22, 8, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public async Task RegisteredBeer_CountsOnlySinceCutoff()
    {
        // ARRANGE
        _beerMock.Setup(d => d.GetBeersAsync()).ReturnsAsync(new List<Beer>
        {
            new() { ID = "a", RegisteredBy = 1, RegisteredAt = Recent },
            new() { ID = "b", RegisteredBy = 2, RegisteredAt = Old },
            new() { ID = "c", RegisteredBy = 3, RegisteredAt = Since },
        });

        // ACT
        var result = await Arrange().GetActiveUserIDsAsync(Since);

        // ASSERT
        Assert.Equal(new[] { 1, 3 }, result);
    }

    [Fact]
    public async Task Rating_CountsByLastChange_AcrossSeasons()
    {
        // ARRANGE
        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season>
        {
            new() { ID = "s1", StartDate = Old.AddMonths(-1), EndDate = Recent },
            new() { ID = "s2", StartDate = Recent },
        });
        _ratingMock.Setup(d => d.GetRatingsAsync("s1")).ReturnsAsync(new List<Rating>
        {
            Rated(1, Old, Recent),       // updated recently
            Rated(2, Old.AddDays(-5), Old), // only old activity
        });
        _ratingMock.Setup(d => d.GetRatingsAsync("s2")).ReturnsAsync(new List<Rating>
        {
            Rated(3, Recent, default),   // no Updated time: Created counts
            Rated(4, Old, default),
        });

        // ACT
        var result = await Arrange().GetActiveUserIDsAsync(Since);

        // ASSERT
        Assert.Equal(new[] { 1, 3 }, result);
    }

    [Fact]
    public async Task Rating_SeasonEndedBeforeCutoff_NotRead()
    {
        // ARRANGE
        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season>
        {
            new() { ID = "old", StartDate = Old.AddMonths(-2), EndDate = Old },
        });

        // ACT
        await Arrange().GetActiveUserIDsAsync(Since);

        // ASSERT
        _ratingMock.Verify(d => d.GetRatingsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChatPosters_Count_WithCutoffPassedThrough()
    {
        // ARRANGE
        _chatMock.Setup(d => d.GetPosterIDsAsync(Since)).ReturnsAsync(new List<int> { 5, 6 });

        // ACT
        var result = await Arrange().GetActiveUserIDsAsync(Since);

        // ASSERT
        Assert.Equal(new[] { 5, 6 }, result);
    }

    [Fact]
    public async Task AllSources_DistinctSortedAndValidOnly()
    {
        // ARRANGE
        _beerMock.Setup(d => d.GetBeersAsync()).ReturnsAsync(new List<Beer>
        {
            new() { ID = "a", RegisteredBy = 9, RegisteredAt = Recent },
            new() { ID = "b", RegisteredBy = 0, RegisteredAt = Recent },
        });
        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season> { new() { ID = "s", StartDate = Old } });
        _ratingMock.Setup(d => d.GetRatingsAsync("s")).ReturnsAsync(new List<Rating> { Rated(9, Recent, Recent), Rated(4, Recent, Recent), Rated(4, Recent, Recent) });
        _chatMock.Setup(d => d.GetPosterIDsAsync(Since)).ReturnsAsync(new List<int> { 4, 2, -1 });

        // ACT
        var result = await Arrange().GetActiveUserIDsAsync(Since);

        // ASSERT
        Assert.Equal(new[] { 2, 4, 9 }, result);
    }

    [Fact]
    public async Task ChatFails_BeersAndRatingsStillCount()
    {
        // ARRANGE
        _beerMock.Setup(d => d.GetBeersAsync()).ReturnsAsync(new List<Beer> { new() { ID = "a", RegisteredBy = 1, RegisteredAt = Recent } });
        _chatMock.Setup(d => d.GetPosterIDsAsync(It.IsAny<DateTime>())).ThrowsAsync(new InvalidOperationException("database down"));

        // ACT
        var result = await Arrange().GetActiveUserIDsAsync(Since);

        // ASSERT
        Assert.Equal(new[] { 1 }, result);
    }
}
