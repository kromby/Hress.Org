namespace Ez.Hress.BeerRater.Entities;

public class BeerSummary
{
    public BeerSummary(Beer beer)
    {
        Beer = beer;
    }

    public Beer Beer { get; set; }

    // The calling user's stars for this beer in the season, or null if they haven't rated it.
    public int? MyRating { get; set; }

    // How many members have rated the beer in the season. The stars stay hidden until the season ends.
    public int RatingCount { get; set; }

    // How many chat messages there are about the beer in the season.
    public int MessageCount { get; set; }
}
