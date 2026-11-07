using System.Globalization;
using System.Security.Cryptography;
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
    private const int MaxShortText = 100;
    private const int MaxUrl = 500;
    private const int MaxDescription = 2000;
    private const double MaxAlcohol = 20;
    private const string IdAlphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int IdLength = 10;

    public async Task<IList<CatalogBeer>> SearchAsync(string? query)
    {
        // The query is free text, so only its length is logged.
        var trimmedLength = query?.Trim().Length ?? 0;
        log.LogInformation("[{Class}.{Method}] Searching the catalog, query length {Length}", nameof(BeerCatalogInteractor), nameof(SearchAsync), trimmedLength);

        // Checked before normalizing too: "æ" and "þ" normalize to two letters.
        var normalizedQuery = BeerNames.Normalize(query);
        if (trimmedLength < MinQueryLength || normalizedQuery.Length < MinQueryLength)
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

    public async Task<Beer> AddToSeasonAsync(string seasonID, string beerID)
    {
        log.LogInformation("[{Class}.{Method}] Adding beer '{BeerID}' to season '{SeasonID}'", nameof(BeerCatalogInteractor), nameof(AddToSeasonAsync), beerID, seasonID);

        await seasonInteractor.GetOpenSeasonAsync(seasonID);

        var beer = (await beerDataAccess.GetBeersAsync()).FirstOrDefault(b => b.ID == beerID)
            ?? throw new KeyNotFoundException($"Beer '{beerID}' not found.");

        await seasonDataAccess.AddBeerToSeasonAsync(seasonID, beerID);
        return beer;
    }

    public async Task<Beer> CreateInSeasonAsync(string seasonID, NewBeer newBeer, int userID)
    {
        log.LogInformation("[{Class}.{Method}] Creating a beer in season '{SeasonID}'", nameof(BeerCatalogInteractor), nameof(CreateInSeasonAsync), seasonID);

        await seasonInteractor.GetOpenSeasonAsync(seasonID);

        var name = Required(newBeer.Name, "Name", MaxShortText);
        var producer = Required(newBeer.Producer, "Producer", MaxShortText);
        var country = Required(newBeer.Country, "Country", MaxShortText);
        var style = Optional(newBeer.Style, "Style", MaxShortText);
        var description = Optional(newBeer.Description, "Description", MaxDescription);
        var url = Optional(newBeer.VinbudinUrl, "VinbudinUrl", MaxUrl);

        if (newBeer.AlcoholPercentage is not > 0 or > MaxAlcohol)
        {
            throw new ArgumentException($"AlcoholPercentage must be greater than 0 and at most {MaxAlcohol}.");
        }

        if (url != null && !(Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps))
        {
            throw new ArgumentException("VinbudinUrl must be an absolute https URL.");
        }

        var normalizedName = BeerNames.Normalize(name);
        var normalizedProducer = BeerNames.Normalize(producer);
        var existing = (await beerDataAccess.GetBeersAsync()).FirstOrDefault(b =>
            BeerNames.Normalize(b.Name) == normalizedName && BeerNames.Normalize(b.Producer) == normalizedProducer);
        if (existing != null)
        {
            throw new DuplicateBeerException(existing);
        }

        var beer = new Beer
        {
            ID = RandomNumberGenerator.GetString(IdAlphabet, IdLength),
            Name = name,
            Producer = producer,
            Country = country,
            AlcoholPercentage = newBeer.AlcoholPercentage.Value,
            VinbudinUrl = url,
            Style = style,
            Description = description,
            ImageID = newBeer.ImageID,
            RegisteredBy = userID,
            RegisteredAt = DateTime.UtcNow
        };

        await beerDataAccess.AddBeerAsync(beer);
        await seasonDataAccess.AddBeerToSeasonAsync(seasonID, beer.ID);
        return beer;
    }

    private static string Required(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw new ArgumentException($"{field} is required.");
        }

        return trimmed.Length > maxLength ? throw new ArgumentException($"{field} must be at most {maxLength} characters.") : trimmed;
    }

    private static string? Optional(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return trimmed.Length > maxLength ? throw new ArgumentException($"{field} must be at most {maxLength} characters.") : trimmed;
    }
}
