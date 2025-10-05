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
    private readonly TranslationService _translationService;
    private readonly ILogger<MovieInteractor> _log;

    public MovieInteractor(IMovieDataAccess movieDataAccess, IMovieInformationDataAccess movieInformationDataAccess, TranslationService translationService, ILogger<MovieInteractor> log)
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

        // Store original values without translation - translations will be applied at retrieval time
        return await _movieInformationDataAccess.SaveMovieInformationAsync(movieInfo);
    }


    public async Task<MovieInfo?> GetMovieInformationAsync(int movieId)
    {
        _log.LogInformation("Getting movie information for ID: {MovieId}", movieId);
        var movieInfo = await _movieInformationDataAccess.GetMovieInformationAsync(movieId);
        
        if (movieInfo != null)
        {
            // Apply translations at retrieval time
            await TranslateMovieInfoAsync(movieInfo);
        }
        
        return movieInfo;
    }

    private async Task TranslateMovieInfoAsync(MovieInfo movieInfo)
    {
        try
        {
            _log.LogInformation("Translating movie information fields for movie: {MovieName}", movieInfo.Name);

            // Use a timeout to prevent hanging on translation failures
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            
            // Translate Country
            if (!string.IsNullOrWhiteSpace(movieInfo.Country))
            {
                var originalCountry = movieInfo.Country;
                try
                {
                    movieInfo.Country = await _translationService.TranslateAsync(movieInfo.Country, "en");
                    _log.LogInformation("Translated Country: '{Original}' -> '{Translated}'", originalCountry, movieInfo.Country);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to translate Country for movie {MovieName}, using original", movieInfo.Name);
                    // Country remains as original value
                }
            }

            // Translate Genre list
            if (movieInfo.Genre != null && movieInfo.Genre.Count > 0)
            {
                var originalGenres = new List<string>(movieInfo.Genre);
                try
                {
                    var translatedGenres = await _translationService.TranslateListAsync(movieInfo.Genre, "en");
                    movieInfo.Genre = translatedGenres;
                    _log.LogInformation("Translated Genres: '{Original}' -> '{Translated}'", string.Join(", ", originalGenres), string.Join(", ", translatedGenres));
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to translate Genres for movie {MovieName}, using original", movieInfo.Name);
                    // Genres remain as original values
                }
            }

            // Translate Language list
            if (movieInfo.Language != null && movieInfo.Language.Count > 0)
            {
                var originalLanguages = new List<string>(movieInfo.Language);
                try
                {
                    var translatedLanguages = await _translationService.TranslateListAsync(movieInfo.Language, "en");
                    movieInfo.Language = translatedLanguages;
                    _log.LogInformation("Translated Languages: '{Original}' -> '{Translated}'", string.Join(", ", originalLanguages), string.Join(", ", translatedLanguages));
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to translate Languages for movie {MovieName}, using original", movieInfo.Name);
                    // Languages remain as original values
                }
            }

            // Translate Awards
            if (!string.IsNullOrWhiteSpace(movieInfo.Awards))
            {
                var originalAwards = movieInfo.Awards;
                try
                {
                    movieInfo.Awards = await _translationService.TranslateAsync(movieInfo.Awards, "en");
                    _log.LogInformation("Translated Awards: '{Original}' -> '{Translated}'", originalAwards, movieInfo.Awards);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to translate Awards for movie {MovieName}, using original", movieInfo.Name);
                    // Awards remain as original value
                }
            }

            _log.LogInformation("Completed translation of movie information for: {MovieName}", movieInfo.Name);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error translating movie information for movie: {MovieName}", movieInfo.Name);
            // Continue with original text if translation fails - this is the fallback strategy
        }
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
