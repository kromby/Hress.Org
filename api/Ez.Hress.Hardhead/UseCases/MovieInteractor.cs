using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

namespace Ez.Hress.Hardhead.UseCases;

public class MovieInteractor
{
    private readonly IMovieDataAccess _movieDataAccess;
    private readonly IMovieInformationDataAccess _movieInformationDataAccess;
    private readonly ITranslationService _translationService;
    private readonly ILogger<MovieInteractor> _log;

    public MovieInteractor(IMovieDataAccess movieDataAccess, IMovieInformationDataAccess movieInformationDataAccess, ITranslationService translationService, ILogger<MovieInteractor> log)
    {
        _movieDataAccess = movieDataAccess;
        _movieInformationDataAccess = movieInformationDataAccess;
        _translationService = translationService;
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

        // Translate movie information fields to Icelandic
        await TranslateMovieInfoAsync(movieInfo);

        return await _movieInformationDataAccess.SaveMovieInformationAsync(movieInfo);
    }

    private async Task TranslateMovieInfoAsync(MovieInfo movieInfo)
    {
        try
        {
            _log.LogInformation("Translating movie information fields for movie: {MovieName}", movieInfo.Name);

            // Translate Country
            if (!string.IsNullOrWhiteSpace(movieInfo.Country))
            {
                var originalCountry = movieInfo.Country;
                movieInfo.Country = await _translationService.TranslateAsync(movieInfo.Country, "en");
                _log.LogInformation("Translated Country: '{Original}' -> '{Translated}'", originalCountry, movieInfo.Country);
            }

            // Translate Genre list
            if (movieInfo.Genre != null && movieInfo.Genre.Count > 0)
            {
                var originalGenres = new List<string>(movieInfo.Genre);
                var translatedGenres = await _translationService.TranslateListAsync(movieInfo.Genre, "en");
                movieInfo.Genre = translatedGenres;
                _log.LogInformation("Translated Genres: '{Original}' -> '{Translated}'", string.Join(", ", originalGenres), string.Join(", ", translatedGenres));
            }

            // Translate Language list
            if (movieInfo.Language != null && movieInfo.Language.Count > 0)
            {
                var originalLanguages = new List<string>(movieInfo.Language);
                var translatedLanguages = await _translationService.TranslateListAsync(movieInfo.Language, "en");
                movieInfo.Language = translatedLanguages;
                _log.LogInformation("Translated Languages: '{Original}' -> '{Translated}'", string.Join(", ", originalLanguages), string.Join(", ", translatedLanguages));
            }

            // Translate Awards
            if (!string.IsNullOrWhiteSpace(movieInfo.Awards))
            {
                var originalAwards = movieInfo.Awards;
                movieInfo.Awards = await _translationService.TranslateAsync(movieInfo.Awards, "en");
                _log.LogInformation("Translated Awards: '{Original}' -> '{Translated}'", originalAwards, movieInfo.Awards);
            }

            _log.LogInformation("Completed translation of movie information for: {MovieName}", movieInfo.Name);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error translating movie information for movie: {MovieName}", movieInfo.Name);
            // Continue with original text if translation fails
        }
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
