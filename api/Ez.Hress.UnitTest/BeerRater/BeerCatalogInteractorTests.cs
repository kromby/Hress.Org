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

    private static NewBeer ValidNewBeer() =>
        new() { Name = "Jóla Gull", Producer = "Ölgerðin", Country = "Ísland", AlcoholPercentage = 5.4 };

    [Fact]
    public async Task AddToSeason_ExistingBeer_AddsAndReturnsIt()
    {
        var result = await Arrange().AddToSeasonAsync("jol-2026", "2");

        Assert.Equal("2", result.ID);
        _seasonMock.Verify(d => d.AddBeerToSeasonAsync("jol-2026", "2"), Times.Once);
    }

    [Fact]
    public async Task AddToSeason_UnknownBeer_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Arrange().AddToSeasonAsync("jol-2026", "nope"));
        _seasonMock.Verify(d => d.AddBeerToSeasonAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddToSeason_ClosedSeason_ThrowsSeasonClosed()
    {
        await Assert.ThrowsAsync<SeasonClosedException>(() => Arrange().AddToSeasonAsync("jol-2025", "2"));
        _seasonMock.Verify(d => d.AddBeerToSeasonAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Create_Valid_SavesTrimmedBeerAndAddsToSeason()
    {
        Beer? saved = null;
        _beerMock.Setup(d => d.AddBeerAsync(It.IsAny<Beer>())).Callback<Beer>(b => saved = b).Returns(Task.CompletedTask);
        var input = new NewBeer { Name = " Jóla Gull ", Producer = "Ölgerðin", Country = "Ísland", AlcoholPercentage = 5.4, Style = "Lager", ImageID = 279080 };

        var result = await Arrange().CreateInSeasonAsync("jol-2026", input, 7);

        Assert.NotNull(saved);
        Assert.Same(saved, result);
        Assert.Equal("Jóla Gull", saved!.Name);
        Assert.Equal("Ölgerðin", saved.Producer);
        Assert.Equal("Lager", saved.Style);
        Assert.Equal(279080, saved.ImageID);
        Assert.Equal(7, saved.RegisteredBy);
        Assert.InRange((DateTime.UtcNow - saved.RegisteredAt).TotalSeconds, -5, 5);
        Assert.Matches("^[a-z0-9]{10}$", saved.ID);
        _seasonMock.Verify(d => d.AddBeerToSeasonAsync("jol-2026", saved.ID), Times.Once);
    }

    [Fact]
    public async Task Create_BlankOptionalFields_SavedAsNull()
    {
        Beer? saved = null;
        _beerMock.Setup(d => d.AddBeerAsync(It.IsAny<Beer>())).Callback<Beer>(b => saved = b).Returns(Task.CompletedTask);
        var input = ValidNewBeer();
        input.Style = "  ";
        input.VinbudinUrl = "";
        input.Description = null;

        await Arrange().CreateInSeasonAsync("jol-2026", input, 7);

        Assert.Null(saved!.Style);
        Assert.Null(saved.VinbudinUrl);
        Assert.Null(saved.Description);
    }

    [Theory]
    [InlineData("", "P", "C", 5.0, null, null, false)]
    [InlineData("N", " ", "C", 5.0, null, null, false)]
    [InlineData("N", "P", "", 5.0, null, null, false)]
    [InlineData("N", "P", "C", null, null, null, false)]
    [InlineData("N", "P", "C", 0.0, null, null, false)]
    [InlineData("N", "P", "C", -1.0, null, null, false)]
    [InlineData("N", "P", "C", 20.1, null, null, false)]
    [InlineData("N", "P", "C", 20.0, null, null, true)]
    [InlineData("NAME101", "P", "C", 5.0, null, null, false)]
    [InlineData("N", "P", "C", 5.0, "DESC2001", null, false)]
    [InlineData("N", "P", "C", 5.0, null, "vinbudin.is/vara/1", false)]
    [InlineData("N", "P", "C", 5.0, null, "http://vinbudin.is/x", false)]
    [InlineData("N", "P", "C", 5.0, null, "https://www.vinbudin.is/x", true)]
    public async Task Create_Invalid_ThrowsArgument(string name, string producer, string country, double? abv, string? description, string? url, bool valid)
    {
        if (name == "NAME101") name = new string('a', 101);
        if (description == "DESC2001") description = new string('a', 2001);
        var input = new NewBeer { Name = name, Producer = producer, Country = country, AlcoholPercentage = abv, Description = description, VinbudinUrl = url };
        _beerMock.Setup(d => d.AddBeerAsync(It.IsAny<Beer>())).Returns(Task.CompletedTask);
        var interactor = Arrange();

        if (valid)
        {
            await interactor.CreateInSeasonAsync("jol-2026", input, 7);
            _beerMock.Verify(d => d.AddBeerAsync(It.IsAny<Beer>()), Times.Once);
            return;
        }

        await Assert.ThrowsAsync<ArgumentException>(() => interactor.CreateInSeasonAsync("jol-2026", input, 7));
        _beerMock.Verify(d => d.AddBeerAsync(It.IsAny<Beer>()), Times.Never);
    }

    [Fact]
    public async Task Create_SameNameAndProducerDifferentSpacingAndCase_ThrowsDuplicate()
    {
        var beers = new List<Beer> { new() { ID = "1", Name = "Jóla Kaldi", Producer = "Bruggsmiðjan Kaldi", Country = "Ísland", AlcoholPercentage = 5 } };
        var input = ValidNewBeer();
        input.Name = "Jóla  Kaldi ";
        input.Producer = "bruggsmidjan kaldi";

        var ex = await Assert.ThrowsAsync<DuplicateBeerException>(() => Arrange(beers).CreateInSeasonAsync("jol-2026", input, 7));

        Assert.Equal("1", ex.Existing.ID);
        _beerMock.Verify(d => d.AddBeerAsync(It.IsAny<Beer>()), Times.Never);
    }

    [Fact]
    public async Task Create_SameNameDifferentProducer_Saves()
    {
        var beers = new List<Beer> { new() { ID = "1", Name = "Jóla Gull", Producer = "Tuborg", Country = "Danmörk", AlcoholPercentage = 5 } };
        _beerMock.Setup(d => d.AddBeerAsync(It.IsAny<Beer>())).Returns(Task.CompletedTask);

        await Arrange(beers).CreateInSeasonAsync("jol-2026", ValidNewBeer(), 7);

        _beerMock.Verify(d => d.AddBeerAsync(It.IsAny<Beer>()), Times.Once);
    }

    [Fact]
    public async Task Create_ClosedSeason_ThrowsSeasonClosed()
    {
        await Assert.ThrowsAsync<SeasonClosedException>(() => Arrange().CreateInSeasonAsync("jol-2025", ValidNewBeer(), 7));
        _beerMock.Verify(d => d.AddBeerAsync(It.IsAny<Beer>()), Times.Never);
    }
}
