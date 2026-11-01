using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.BeerRater;

public class SeasonInteractorTests
{
    private readonly Mock<IBeerSeasonDataAccess> _dataMock;
    private readonly Mock<ILogger<SeasonInteractor>> _log;

    public SeasonInteractorTests()
    {
        _dataMock = new Mock<IBeerSeasonDataAccess>();
        _log = new Mock<ILogger<SeasonInteractor>>();
    }

    private static Season NewSeason(string id, DateTime start, DateTime? end) =>
        new() { ID = id, Name = id, StartDate = start, EndDate = end };

    private SeasonInteractor Arrange(params Season[] seasons)
    {
        _dataMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(seasons.ToList());
        return new SeasonInteractor(_dataMock.Object, _log.Object);
    }

    [Fact]
    public async Task CurrentSeason_ReturnsRunningSeason()
    {
        // ARRANGE
        var interactor = Arrange(
            NewSeason("jolabjor-2025", new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc)),
            NewSeason("jolabjor-2026", new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), null));

        // ACT
        var result = await interactor.GetCurrentSeasonAsync();

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal("jolabjor-2026", result.ID);
        Assert.True(result.IsOpen);
    }

    [Fact]
    public async Task CurrentSeason_TwoRunning_ReturnsLatestStart()
    {
        // ARRANGE
        var interactor = Arrange(
            NewSeason("older", new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), null),
            NewSeason("newer", new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), null));

        // ACT
        var result = await interactor.GetCurrentSeasonAsync();

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal("newer", result.ID);
    }

    [Fact]
    public async Task CurrentSeason_NoneRunning_ReturnsLatestEnded()
    {
        // ARRANGE
        var interactor = Arrange(
            NewSeason("jolabjor-2024", new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 6, 0, 0, 0, DateTimeKind.Utc)),
            NewSeason("jolabjor-2025", new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc)));

        // ACT
        var result = await interactor.GetCurrentSeasonAsync();

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal("jolabjor-2025", result.ID);
        Assert.False(result.IsOpen);
    }

    [Fact]
    public async Task CurrentSeason_NoSeasons_ReturnsNull()
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT
        var result = await interactor.GetCurrentSeasonAsync();

        // ASSERT
        Assert.Null(result);
    }

    [Fact]
    public async Task OpenSeason_Closed_ThrowsInvalidOperation()
    {
        // ARRANGE
        var interactor = Arrange(
            NewSeason("jolabjor-2025", new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc)));

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => interactor.GetOpenSeasonAsync("jolabjor-2025"));
    }

    [Fact]
    public async Task OpenSeason_Unknown_ThrowsKeyNotFound()
    {
        // ARRANGE
        var interactor = Arrange(
            NewSeason("jolabjor-2026", new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), null));

        // ACT & ASSERT
        await Assert.ThrowsAsync<KeyNotFoundException>(() => interactor.GetOpenSeasonAsync("nope"));
    }
}
