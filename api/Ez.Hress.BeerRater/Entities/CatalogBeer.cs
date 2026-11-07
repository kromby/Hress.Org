namespace Ez.Hress.BeerRater.Entities;

// A beer in the catalog, with where it was last rated.
public class CatalogBeer
{
    public Beer Beer { get; set; } = new();

    public string? LatestSeasonID { get; set; }

    public string? LatestSeasonName { get; set; }

    // True when the beer is already in the running season.
    public bool InCurrentSeason { get; set; }
}
