using System.Globalization;
using Ez.Hress.BeerRater.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.UseCases;

public class SummaryInteractor
{
    private static readonly StringComparer _nameComparer = StringComparer.Create(new CultureInfo("is-IS"), ignoreCase: true);

    private readonly BeerInteractor _beerInteractor;
    private readonly IBeerRatingDataAccess _ratingDataAccess;
    private readonly ILogger<SummaryInteractor> _log;

    public SummaryInteractor(BeerInteractor beerInteractor, IBeerRatingDataAccess ratingDataAccess, ILogger<SummaryInteractor> log)
    {
        _beerInteractor = beerInteractor;
        _ratingDataAccess = ratingDataAccess;
        _log = log;
    }

    public async Task<IList<BeerSummary>> GetSeasonSummariesAsync(string seasonID, int userID)
    {
        _log.LogInformation("[{Class}.{Method}] Getting beer summaries for season '{SeasonID}'", nameof(SummaryInteractor), nameof(GetSeasonSummariesAsync), seasonID);

        var beers = await _beerInteractor.GetSeasonBeersAsync(seasonID);
        var ratings = await _ratingDataAccess.GetRatingsAsync(seasonID);
        var myRatings = ratings
            .Where(r => r.UserID == userID)
            .GroupBy(r => r.BeerID, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Stars, StringComparer.Ordinal);

        return beers
            .OrderBy(b => b.Name, _nameComparer)
            .Select(b => new BeerSummary(b) { MyRating = myRatings.TryGetValue(b.ID, out var stars) ? stars : null })
            .ToList();
    }
}
