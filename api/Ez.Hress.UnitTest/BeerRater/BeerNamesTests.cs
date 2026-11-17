using Ez.Hress.BeerRater.UseCases;

namespace Ez.Hress.UnitTest.BeerRater;

public class BeerNamesTests
{
    [Theory]
    [InlineData("  Jóla   Kaldi ", "jola kaldi")]
    [InlineData("Ölvisholt", "olvisholt")]
    [InlineData("Garðar Þór Æði", "gardar thor aedi")]
    [InlineData(null, "")]
    public void Normalize_FoldsCaseDiacriticsAndWhitespace(string? input, string expected)
    {
        Assert.Equal(expected, BeerNames.Normalize(input));
    }
}
