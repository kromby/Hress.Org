using Ez.Hress.BeerRater.Entities;
using Ez.Hress.BeerRater.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.BeerRater;

public class BeerCatalogInteractorTests
{
    private readonly Mock<IBeerDataAccess> _beerMock = new();
    private readonly Mock<IBeerSeasonDataAccess> _seasonMock = new();

    private static Beer NewBeer(string id, string name) =>
        new() { ID = id, Name = name, Producer = "Carlsberg", Country = "Danmörk", AlcoholPercentage = 5.6 };

    private BeerCatalogInteractor Arrange(IList<Beer>? beers = null)
    {
        beers ??= new List<Beer>
        {
            NewBeer("1", "Jóla Kaldi"),
            NewBeer("2", "Jólabjór"),
            NewBeer("3", "Tuborg Lille Jul"),
            NewBeer("4", "Egils Malt")
        };
        _beerMock.Setup(d => d.GetBeersAsync()).ReturnsAsync(beers);
        _seasonMock.Setup(d => d.GetSeasonsAsync()).ReturnsAsync(new List<Season>
        {
            new() { ID = "jol-2025", Name = "Jólabjór 2025", StartDate = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new() { ID = "jol-2026", Name = "Jólabjór 2026", StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc) }
        });
        _seasonMock.Setup(d => d.GetSeasonBeerIDsAsync("jol-2025")).ReturnsAsync(new List<string> { "1", "2" });
        _seasonMock.Setup(d => d.GetSeasonBeerIDsAsync("jol-2026")).ReturnsAsync(new List<string> { "1", "3" });

        var seasonInteractor = new SeasonInteractor(_seasonMock.Object, new Mock<ILogger<SeasonInteractor>>().Object);
        return new BeerCatalogInteractor(_beerMock.Object, _seasonMock.Object, seasonInteractor, new Mock<ILogger<BeerCatalogInteractor>>().Object);
    }

    [Fact]
    public async Task Search_IgnoresCaseAndDiacritics()
    {
        // Listed out of order, so the result proves the Icelandic sort.
        var beers = new List<Beer> { NewBeer("2", "Jólabjór"), NewBeer("3", "Tuborg Lille Jul"), NewBeer("1", "Jóla Kaldi") };

        var result = await Arrange(beers).SearchAsync("JOLA");

        Assert.Equal(new[] { "1", "2" }, result.Select(r => r.Beer.ID));
    }

    [Fact]
    public async Task Search_SaysLatestSeasonAndWhetherInCurrent()
    {
        var interactor = Arrange();

        var kaldi = Assert.Single(await interactor.SearchAsync("kaldi"));
        Assert.Equal("jol-2026", kaldi.LatestSeasonID);
        Assert.Equal("Jólabjór 2026", kaldi.LatestSeasonName);
        Assert.True(kaldi.InCurrentSeason);

        var bjor = Assert.Single(await interactor.SearchAsync("jolabjor"));
        Assert.Equal("jol-2025", bjor.LatestSeasonID);
        Assert.False(bjor.InCurrentSeason);
    }

    [Fact]
    public async Task Search_BeerInNoSeason_HasNoLatestSeason()
    {
        var egils = Assert.Single(await Arrange().SearchAsync("egils"));

        Assert.Null(egils.LatestSeasonID);
        Assert.False(egils.InCurrentSeason);
    }

    [Theory]
    [InlineData("j")]
    [InlineData("æ")]
    [InlineData(" þ ")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task Search_ShortOrBlankQuery_ReturnsEmptyWithoutReading(string? query)
    {
        var result = await Arrange().SearchAsync(query);

        Assert.Empty(result);
        _beerMock.Verify(d => d.GetBeersAsync(), Times.Never);
    }

    [Fact]
    public async Task Search_ReturnsAtMostTen()
    {
        // Listed in reverse, so the result proves it sorts before taking ten.
        var beers = Enumerable.Range(1, 12).Reverse().Select(i => NewBeer(i.ToString(), $"Jól {i:00}")).ToList();

        var result = await Arrange(beers).SearchAsync("jol");

        Assert.Equal(Enumerable.Range(1, 10).Select(i => i.ToString()), result.Select(r => r.Beer.ID));
    }
}
