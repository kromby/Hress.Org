using Ez.Hress.Hardhead.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

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

    public async Task<Movie> GetMovieAsync(int id)
    {
        _log.LogInformation("Getting movie by ID: {ID}", id);
        return await _movieDataAccess.GetMovie(id);
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

    public async Task<MovieInfo?> GetMovieInformationAsync(int movieId)
    {
        _log.LogInformation("Getting movie information for ID: {MovieId}", movieId);
        return await _movieInformationDataAccess.GetMovieInformationAsync(movieId);
    }

    /// <summary>
    /// Updates a movie with the provided information.
    /// </summary>
    /// <param name="id">The ID of the movie to update.</param>
    /// <param name="userID">The ID of the user making the update.</param>
    /// <param name="movie">The updated movie information.</param>
    /// <returns>True if the update was successful, false otherwise.</returns>
    public async Task<bool> UpdateMovieAsync(int id, int userID, Movie movie)
    {
        _log.LogInformation("Updating movie with ID {ID}: {Movie}", id, movie);
        
        if (movie == null)
            throw new ArgumentException("Can not be null.", nameof(movie));

        movie.Validate();

        if (!string.IsNullOrWhiteSpace(movie.ImdbUrl) && !movie.ImdbUrl.StartsWith("http", true, CultureInfo.InvariantCulture))
            movie.ImdbUrl = string.Format(CultureInfo.InvariantCulture, $"https://{movie.ImdbUrl}");

        if (!string.IsNullOrWhiteSpace(movie.YoutubeUrl))
        {
            var youtubeUrlString = movie.YoutubeUrl;

            if (!youtubeUrlString.StartsWith("http", true, CultureInfo.InvariantCulture))
                youtubeUrlString = $"https://{movie.YoutubeUrl}";

            if (!youtubeUrlString.Contains("v=", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("Youtube URL is invalid.", nameof(movie));

            try
            {
                var youtubeUrl = new Uri(youtubeUrlString);
                movie.YoutubeUrl = youtubeUrl.Query.Contains(";")
                    ? youtubeUrl.Query.Substring(youtubeUrl.Query.IndexOf("v=", StringComparison.InvariantCultureIgnoreCase) + 2, youtubeUrl.Query.IndexOf(";", StringComparison.InvariantCultureIgnoreCase))
                    : youtubeUrl.Query.Substring(youtubeUrl.Query.IndexOf("v=", StringComparison.InvariantCultureIgnoreCase) + 2);
            }
            catch (UriFormatException ufex)
            {
                throw new ArgumentException($"Youtube URL is invalid: {ufex.Message}.", nameof(movie));
            }
        }

        //if (movie.PosterPhoto != null && !string.IsNullOrEmpty(movie.PosterPhoto.Href))
        //{
        //    movie.PosterPhotoID = movie.PosterPhoto.ID;
        //}

        movie.Updated = DateTime.UtcNow;
        movie.UpdatedBy = userID;
        
        return await _movieDataAccess.UpdateMovie(id, userID, movie);
    }
}
