using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.BeerRater;

public class RatingInteractorTests
{
    private const string SeasonID = "jolabjor-2026";
    private const string BeerID = "b1";
    private const int UserID = 7;

    private readonly Mock<IBeerDataAccess> _beerMock;
    private readonly Mock<IBeerSeasonDataAccess> _seasonMock;
    private readonly Mock<IBeerRatingDataAccess> _ratingMock;

    public RatingInteractorTests()
    {
        _beerMock = new Mock<IBeerDataAccess>();
        _seasonMock = new Mock<IBeerSeasonDataAccess>();
        _ratingMock = new Mock<IBeerRatingDataAccess>();

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

    private RatingInteractor Arrange()
    {
        var seasonInteractor = new SeasonInteractor(_seasonMock.Object, new Mock<ILogger<SeasonInteractor>>().Object);
        var beerInteractor = new BeerInteractor(_beerMock.Object, _seasonMock.Object, seasonInteractor, new Mock<ILogger<BeerInteractor>>().Object);
        return new RatingInteractor(_ratingMock.Object, beerInteractor, seasonInteractor, new Mock<ILogger<RatingInteractor>>().Object);
    }

    [Fact]
    public async Task Save_NewRating_Upserts()
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT
        var result = await interactor.SaveRatingAsync(SeasonID, BeerID, UserID, 4);

        // ASSERT
        _ratingMock.Verify(d => d.UpsertRatingAsync(It.Is<Rating>(r =>
            r.SeasonID == SeasonID && r.BeerID == BeerID && r.UserID == UserID && r.Stars == 4)), Times.Once);
        Assert.Equal(4, result.Stars);
        Assert.Equal(result.Created, result.Updated);
        Assert.Equal(DateTimeKind.Utc, result.Created.Kind);
    }

    [Fact]
    public async Task Save_ExistingRating_KeepsCreated_UpdatesUpdatedAndStars()
    {
        // ARRANGE
        var created = new DateTime(2026, 11, 2, 20, 0, 0, DateTimeKind.Utc);
        _ratingMock.Setup(d => d.GetRatingAsync(SeasonID, BeerID, UserID)).ReturnsAsync(
            new Rating { SeasonID = SeasonID, BeerID = BeerID, UserID = UserID, Stars = 2, Created = created, Updated = created });
        var interactor = Arrange();

        // ACT
        var result = await interactor.SaveRatingAsync(SeasonID, BeerID, UserID, 5);

        // ASSERT
        Assert.Equal(5, result.Stars);
        Assert.Equal(created, result.Created);
        Assert.True(result.Updated > created);
        _ratingMock.Verify(d => d.UpsertRatingAsync(It.Is<Rating>(r => r.Stars == 5 && r.Created == created)), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task Save_StarsOutOfRange_ThrowsArgument(int stars)
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAnyAsync<ArgumentException>(() => interactor.SaveRatingAsync(SeasonID, BeerID, UserID, stars));
        _ratingMock.Verify(d => d.UpsertRatingAsync(It.IsAny<Rating>()), Times.Never);
    }

    [Fact]
    public async Task Save_ClosedSeason_ThrowsSeasonClosed()
    {
        // ARRANGE
        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season>
        {
            new() { ID = SeasonID, StartDate = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc) }
        });
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAsync<SeasonClosedException>(() => interactor.SaveRatingAsync(SeasonID, BeerID, UserID, 4));
        _ratingMock.Verify(d => d.UpsertRatingAsync(It.IsAny<Rating>()), Times.Never);
    }

    [Fact]
    public async Task Save_BeerNotInSeason_ThrowsKeyNotFound()
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAsync<KeyNotFoundException>(() => interactor.SaveRatingAsync(SeasonID, "old", UserID, 4));
        _ratingMock.Verify(d => d.UpsertRatingAsync(It.IsAny<Rating>()), Times.Never);
    }

    [Fact]
    public async Task Save_UnknownSeason_ThrowsKeyNotFound()
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAsync<KeyNotFoundException>(() => interactor.SaveRatingAsync("nope", BeerID, UserID, 4));
    }
}
