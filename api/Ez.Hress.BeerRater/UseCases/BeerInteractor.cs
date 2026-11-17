using Ez.Hress.BeerRater.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.UseCases;

public class BeerInteractor
{
    private readonly IBeerDataAccess _beerDataAccess;
    private readonly IBeerSeasonDataAccess _seasonDataAccess;
    private readonly SeasonInteractor _seasonInteractor;
    private readonly ILogger<BeerInteractor> _log;

    public BeerInteractor(IBeerDataAccess beerDataAccess, IBeerSeasonDataAccess seasonDataAccess, SeasonInteractor seasonInteractor, ILogger<BeerInteractor> log)
    {
        _beerDataAccess = beerDataAccess;
        _seasonDataAccess = seasonDataAccess;
        _seasonInteractor = seasonInteractor;
        _log = log;
    }

    public async Task<IList<Beer>> GetSeasonBeersAsync(string seasonID)
    {
        _log.LogInformation("[{Class}.{Method}] Getting beers for season '{SeasonID}'", nameof(BeerInteractor), nameof(GetSeasonBeersAsync), seasonID);

        // Throws KeyNotFoundException for an unknown season.
        await _seasonInteractor.GetSeasonAsync(seasonID);

        var seasonBeerIDs = new HashSet<string>(await _seasonDataAccess.GetSeasonBeerIDsAsync(seasonID), StringComparer.Ordinal);
        var beers = await _beerDataAccess.GetBeersAsync();

        // A season row whose beer has been removed is skipped.
        return beers.Where(b => seasonBeerIDs.Contains(b.ID)).ToList();
    }

    public async Task<Beer> GetSeasonBeerAsync(string seasonID, string beerID)
    {
        var beers = await GetSeasonBeersAsync(seasonID);
        var beer = beers.FirstOrDefault(b => string.Equals(b.ID, beerID, StringComparison.Ordinal));
        if (beer == null)
            throw new KeyNotFoundException($"Beer '{beerID}' not found in season '{seasonID}'.");

        return beer;
    }
}
