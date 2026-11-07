using System.Globalization;
using Ez.Hress.BeerRater.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.UseCases;

public class BeerCatalogInteractor(
    IBeerDataAccess beerDataAccess,
    IBeerSeasonDataAccess seasonDataAccess,
    SeasonInteractor seasonInteractor,
    ILogger<BeerCatalogInteractor> log)
{
    private const int MinQueryLength = 2;
    private const int MaxResults = 10;

    public async Task<IList<CatalogBeer>> SearchAsync(string? query)
    {
        log.LogInformation("[{Class}.{Method}] Searching the catalog for '{Query}'", nameof(BeerCatalogInteractor), nameof(SearchAsync), query);

        var normalizedQuery = BeerNames.Normalize(query);
        if (normalizedQuery.Length < MinQueryLength)
        {
            return new List<CatalogBeer>();
        }

        var comparer = StringComparer.Create(new CultureInfo("is-IS"), ignoreCase: true);
        var matches = (await beerDataAccess.GetBeersAsync())
            .Where(b => BeerNames.Normalize(b.Name).Contains(normalizedQuery, StringComparison.Ordinal))
            .OrderBy(b => b.Name, comparer)
            .Take(MaxResults)
            .ToList();
        if (matches.Count == 0)
        {
            return new List<CatalogBeer>();
        }

        // Latest season per beer: the one with the latest StartDate.
        var seasons = (await seasonDataAccess.GetSeasonsAsync()).OrderBy(s => s.StartDate).ToList();
        var latestSeasonByBeer = new Dictionary<string, Season>(StringComparer.Ordinal);
        foreach (var season in seasons)
        {
            foreach (var beerID in await seasonDataAccess.GetSeasonBeerIDsAsync(season.ID))
            {
                latestSeasonByBeer[beerID] = season;
            }
        }

        var current = await seasonInteractor.GetCurrentSeasonAsync();
        var currentID = current is { IsOpen: true } ? current.ID : null;

        return matches.Select(beer =>
        {
            latestSeasonByBeer.TryGetValue(beer.ID, out var latest);
            return new CatalogBeer
            {
                Beer = beer,
                LatestSeasonID = latest?.ID,
                LatestSeasonName = latest?.Name,
                InCurrentSeason = currentID != null && latest?.ID == currentID
            };
        }).ToList();
    }
}
