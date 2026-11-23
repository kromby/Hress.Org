using Ez.Hress.BeerRater.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.UseCases;

// Who uses Ölrýnir: members who registered a beer, rated a beer or posted in the chat recently.
public class BeerActivityInteractor
{
    // How far back activity counts.
    public const int ActiveMonths = 6;

    private readonly IBeerDataAccess _beerDataAccess;
    private readonly IBeerSeasonDataAccess _seasonDataAccess;
    private readonly IBeerRatingDataAccess _ratingDataAccess;
    private readonly IBeerChatDataAccess _chatDataAccess;
    private readonly ILogger<BeerActivityInteractor> _log;

    public BeerActivityInteractor(IBeerDataAccess beerDataAccess, IBeerSeasonDataAccess seasonDataAccess, IBeerRatingDataAccess ratingDataAccess, IBeerChatDataAccess chatDataAccess, ILogger<BeerActivityInteractor> log)
    {
        _beerDataAccess = beerDataAccess;
        _seasonDataAccess = seasonDataAccess;
        _ratingDataAccess = ratingDataAccess;
        _chatDataAccess = chatDataAccess;
        _log = log;
    }

    // The start of the active period that ends at 'now' (UTC).
    public static DateTime ActiveSince(DateTime now) => now.AddMonths(-ActiveMonths);

    // The distinct IDs, ascending, of members who since 'since' (UTC) registered a beer, rated a beer in
    // any season or posted in the Ölrýnir chat.
    public async Task<IList<int>> GetActiveUserIDsAsync(DateTime since)
    {
        _log.LogInformation("[{Class}.{Method}] Getting members active since {Since}", nameof(BeerActivityInteractor), nameof(GetActiveUserIDsAsync), since);

        var registered = GetRegisteredByAsync(since);
        var rated = GetRatedByAsync(since);
        var posted = GetPostedByAsync(since);
        await Task.WhenAll(registered, rated, posted);

        return (await registered)
            .Concat(await rated)
            .Concat(await posted)
            .Where(id => id > 0)
            .Distinct()
            .Order()
            .ToList();
    }

    private async Task<IEnumerable<int>> GetRegisteredByAsync(DateTime since)
    {
        var beers = await _beerDataAccess.GetBeersAsync();
        return beers.Where(b => b.RegisteredAt >= since).Select(b => b.RegisteredBy);
    }

    private async Task<IEnumerable<int>> GetRatedByAsync(DateTime since)
    {
        // A season that ended before 'since' can't have ratings from after it.
        var seasons = (await _seasonDataAccess.GetSeasonsAsync())
            .Where(s => s.EndDate == null || s.EndDate >= since)
            .ToList();

        var ratings = await Task.WhenAll(seasons.Select(s => _ratingDataAccess.GetRatingsAsync(s.ID)));
        return ratings
            .SelectMany(r => r)
            .Where(r => LastChanged(r) >= since)
            .Select(r => r.UserID);
    }

    // Older ratings may have no Updated time.
    private static DateTime LastChanged(Rating rating) => rating.Updated != default ? rating.Updated : rating.Created;

    // The chat lives in the Hress database. If it can't be reached, beers and ratings still count.
    private async Task<IEnumerable<int>> GetPostedByAsync(DateTime since)
    {
        try
        {
            return await _chatDataAccess.GetPosterIDsAsync(since);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Couldn't get chat posters since {Since}", nameof(BeerActivityInteractor), nameof(GetPostedByAsync), since);
            return [];
        }
    }
}
