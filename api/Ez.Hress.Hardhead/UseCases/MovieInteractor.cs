using Ez.Hress.Hardhead.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Hardhead.UseCases;

public class MovieInteractor
{
    private readonly IMovieDataAccess _movieDataAccess;
    private readonly IMovieInformationDataAccess _movieInformationDataAccess;
    private readonly ILogger<MovieInteractor> _log;

    public MovieInteractor(IMovieDataAccess movieDataAccess, IMovieInformationDataAccess movieInformationDataAccess, ILogger<MovieInteractor> log)
    {
        _movieDataAccess = movieDataAccess;
        _movieInformationDataAccess = movieInformationDataAccess;
        _log = log;
    }

    public Task<IList<Movie>> GetMoviesAsync(string filterBy)
    {
        _log.LogInformation("Getting movies by filter: {Filter}", filterBy);
        return _movieDataAccess.GetMovies(filterBy);
    }

    public async Task<StatsEntity> GetActorStatisticsAsync(PeriodType periodType)
    {
        var entity = new StatsEntity("Harðhaus")
        {
            PeriodType = periodType,
            DateFrom = Utility.GetDateFromPeriodType(periodType)
        };
        entity.List = await _movieDataAccess.GetActorStatistic(entity.DateFrom);

        return entity;
    }

    public async Task<bool> SaveMovieInformationAsync(int hardheadID, int userID, DateTime hardheadDate, MovieInfo movieInfo)
    {
        _log.LogInformation("Saving movie information: {MovieInfo}", movieInfo);
        movieInfo.ID = hardheadID;
        movieInfo.InsertedBy = userID;
        movieInfo.Inserted = DateTime.UtcNow;
        movieInfo.Age = hardheadDate.Subtract(movieInfo.Released).Days / 365;

        return await _movieInformationDataAccess.SaveMovieInformationAsync(movieInfo);
    }
}
