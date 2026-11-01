using Ez.Hress.BeerRater.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.UseCases;

public class SeasonInteractor
{
    private readonly IBeerSeasonDataAccess _dataAccess;
    private readonly ILogger<SeasonInteractor> _log;

    public SeasonInteractor(IBeerSeasonDataAccess dataAccess, ILogger<SeasonInteractor> log)
    {
        _dataAccess = dataAccess;
        _log = log;
    }

    public async Task<Season?> GetCurrentSeasonAsync()
    {
        _log.LogInformation("[{Class}.{Method}] Getting current season", nameof(SeasonInteractor), nameof(GetCurrentSeasonAsync));

        var seasons = await _dataAccess.GetSeasonsAsync();

        var running = seasons.Where(s => s.IsOpen).OrderByDescending(s => s.StartDate).FirstOrDefault();
        if (running != null)
            return running;

        return seasons.OrderByDescending(s => s.StartDate).FirstOrDefault();
    }

    public async Task<Season> GetSeasonAsync(string seasonID)
    {
        _log.LogInformation("[{Class}.{Method}] Getting season '{SeasonID}'", nameof(SeasonInteractor), nameof(GetSeasonAsync), seasonID);

        var seasons = await _dataAccess.GetSeasonsAsync();
        var season = seasons.FirstOrDefault(s => string.Equals(s.ID, seasonID, StringComparison.Ordinal));
        if (season == null)
            throw new KeyNotFoundException($"Season '{seasonID}' not found.");

        return season;
    }

    public async Task<Season> GetOpenSeasonAsync(string seasonID)
    {
        var season = await GetSeasonAsync(seasonID);
        if (!season.IsOpen)
            throw new SeasonClosedException(seasonID);

        return season;
    }
}
