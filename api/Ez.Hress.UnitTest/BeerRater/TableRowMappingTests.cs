using Azure.Data.Tables;
using Ez.Hress.BeerRater.DataAccess;

namespace Ez.Hress.UnitTest.BeerRater;

// Beers and seasons are entered by hand in Storage Explorer, which stores new
// properties as String unless another type is picked.
public class TableRowMappingTests
{
    private static TableEntity BeerRow() => new("beer", "31696")
    {
        ["Name"] = "Tuborg Lille Jul",
        ["Producer"] = "Ölgerðin Egill Skallagrímsson",
        ["Country"] = "Ísland",
    };

    [Fact]
    public void Beer_TypedValues_Maps()
    {
        // ARRANGE - the SDK returns Edm.DateTime as DateTimeOffset on a TableEntity.
        var row = BeerRow();
        row["AlcoholPercentage"] = 4.8;
        row["RegisteredBy"] = 2630;
        row["RegisteredAt"] = new DateTimeOffset(2026, 11, 3, 0, 0, 0, TimeSpan.Zero);
        row["VinbudinUrl"] = "https://www.vinbudin.is/heim/vorur/stoek-vara.aspx/?productid=31696";

        // ACT
        var beer = BeerTableEntity.ToBeer(row);

        // ASSERT
        Assert.Equal("31696", beer.ID);
        Assert.Equal("Tuborg Lille Jul", beer.Name);
        Assert.Equal("Ölgerðin Egill Skallagrímsson", beer.Producer);
        Assert.Equal("Ísland", beer.Country);
        Assert.Equal(4.8, beer.AlcoholPercentage);
        Assert.Equal(2630, beer.RegisteredBy);
        Assert.Equal(new DateTime(2026, 11, 3, 0, 0, 0, DateTimeKind.Utc), beer.RegisteredAt);
        Assert.Equal(DateTimeKind.Utc, beer.RegisteredAt.Kind);
        Assert.Equal("https://www.vinbudin.is/heim/vorur/stoek-vara.aspx/?productid=31696", beer.VinbudinUrl);
    }

    [Theory]
    [InlineData("4.8")]
    [InlineData("4,8")]
    [InlineData(" 4.8 ")]
    public void Beer_StringValues_AreConverted(string alcohol)
    {
        // ARRANGE
        var row = BeerRow();
        row["AlcoholPercentage"] = alcohol;
        row["RegisteredBy"] = "2630";
        row["RegisteredAt"] = "2026-11-03T00:00:00Z";

        // ACT
        var beer = BeerTableEntity.ToBeer(row);

        // ASSERT
        Assert.Equal(4.8, beer.AlcoholPercentage);
        Assert.Equal(2630, beer.RegisteredBy);
        Assert.Equal(new DateTime(2026, 11, 3, 0, 0, 0, DateTimeKind.Utc), beer.RegisteredAt);
        Assert.Equal(DateTimeKind.Utc, beer.RegisteredAt.Kind);
    }

    [Fact]
    public void Beer_Int64AndIntegerAlcohol_AreConverted()
    {
        // ARRANGE
        var row = BeerRow();
        row["AlcoholPercentage"] = 5;
        row["RegisteredBy"] = 2630L;

        // ACT
        var beer = BeerTableEntity.ToBeer(row);

        // ASSERT
        Assert.Equal(5.0, beer.AlcoholPercentage);
        Assert.Equal(2630, beer.RegisteredBy);
    }

    [Fact]
    public void Beer_MissingOptionalValues_UseDefaults()
    {
        // ACT
        var beer = BeerTableEntity.ToBeer(BeerRow());

        // ASSERT
        Assert.Null(beer.VinbudinUrl);
        Assert.Equal(0, beer.AlcoholPercentage);
        Assert.Equal(0, beer.RegisteredBy);
    }

    [Fact]
    public void Beer_EmptyVinbudinUrl_IsNull()
    {
        // ARRANGE
        var row = BeerRow();
        row["VinbudinUrl"] = "  ";

        // ACT
        var beer = BeerTableEntity.ToBeer(row);

        // ASSERT
        Assert.Null(beer.VinbudinUrl);
    }

    [Theory]
    [InlineData("RegisteredBy", "Guðjón")]
    [InlineData("AlcoholPercentage", "sterkur")]
    [InlineData("RegisteredAt", "í gær")]
    public void Beer_UnconvertibleValue_ThrowsFormatNamingTheProperty(string property, string value)
    {
        // ARRANGE
        var row = BeerRow();
        row[property] = value;

        // ACT & ASSERT
        var ex = Assert.Throws<FormatException>(() => BeerTableEntity.ToBeer(row));
        Assert.Contains(property, ex.Message);
    }

    [Fact]
    public void Season_StringValues_AreConverted()
    {
        // ARRANGE
        var row = new TableEntity("season", "jolabjor-2026")
        {
            ["Name"] = "Jólabjór",
            ["TypeID"] = "3",
            ["StartDate"] = "2026-11-01T12:00:00Z",
            ["EndDate"] = "",
        };

        // ACT
        var season = SeasonTableEntity.ToSeason(row);

        // ASSERT
        Assert.Equal("jolabjor-2026", season.ID);
        Assert.Equal("Jólabjór", season.Name);
        Assert.Equal(3, season.TypeID);
        Assert.Equal(new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc), season.StartDate);
        Assert.Null(season.EndDate);
        Assert.True(season.IsOpen);
    }

    [Fact]
    public void Season_TypedValues_Maps()
    {
        // ARRANGE
        var row = new TableEntity("season", "jolabjor-2025")
        {
            ["Name"] = "Jólabjór 2025",
            ["StartDate"] = new DateTimeOffset(2025, 11, 1, 0, 0, 0, TimeSpan.Zero),
            ["EndDate"] = new DateTimeOffset(2026, 1, 6, 0, 0, 0, TimeSpan.Zero),
        };

        // ACT
        var season = SeasonTableEntity.ToSeason(row);

        // ASSERT
        Assert.Null(season.TypeID);
        Assert.Equal(new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc), season.EndDate);
        Assert.Equal(DateTimeKind.Utc, season.EndDate!.Value.Kind);
        Assert.False(season.IsOpen);
    }
}
