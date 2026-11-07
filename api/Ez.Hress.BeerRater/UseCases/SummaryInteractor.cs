using System.Globalization;
using Ez.Hress.BeerRater.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.UseCases;

public class SummaryInteractor
{
    private static readonly StringComparer _nameComparer = StringComparer.Create(new CultureInfo("is-IS"), ignoreCase: true);

    private readonly BeerInteractor _beerInteractor;
    private readonly IBeerRatingDataAccess _ratingDataAccess;
    private readonly BeerChatInteractor _chatInteractor;
    private readonly ILogger<SummaryInteractor> _log;

    public SummaryInteractor(BeerInteractor beerInteractor, IBeerRatingDataAccess ratingDataAccess, BeerChatInteractor chatInteractor, ILogger<SummaryInteractor> log)
    {
        _beerInteractor = beerInteractor;
        _ratingDataAccess = ratingDataAccess;
        _chatInteractor = chatInteractor;
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
        var ratingCounts = ratings
            .GroupBy(r => r.BeerID, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        var messageCounts = await GetMessageCountsAsync(seasonID);

        return beers
            .OrderBy(b => b.Name, _nameComparer)
            .Select(b => new BeerSummary(b)
            {
                MyRating = myRatings.TryGetValue(b.ID, out var stars) ? stars : null,
                RatingCount = ratingCounts.GetValueOrDefault(b.ID),
                MessageCount = messageCounts.TryGetValue(b.ID, out var messages) ? messages : 0,
            })
            .ToList();
    }

    // The chat lives in the Hress database. If it can't be reached the beer list still loads,
    // with no message counts.
    private async Task<IDictionary<string, int>> GetMessageCountsAsync(string seasonID)
    {
        try
        {
            return await _chatInteractor.GetMessageCountsAsync(seasonID);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Couldn't count chat messages for season '{SeasonID}'", nameof(SummaryInteractor), nameof(GetMessageCountsAsync), seasonID);
            return new Dictionary<string, int>(StringComparer.Ordinal);
        }
    }
}
