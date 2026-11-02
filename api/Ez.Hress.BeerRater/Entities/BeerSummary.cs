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
}
