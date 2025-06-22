using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public interface IMovieDataAccess
{
    /// <summary>
    /// Retrieves movies by a partial name.
    /// </summary>
    /// <param name="nameAndActorFilter">Partial name of film or actor.</param>
    /// <returns>A list of movies.</returns>
    Task<IList<Movie>> GetMovies(string nameAndActorFilter);

    /// <summary>
    /// Retrieves a movie by its ID.
    /// </summary>
    /// <param name="id">The ID of the movie to retrieve.</param>
    /// <returns>The movie if found, null otherwise.</returns>
    Task<Movie> GetMovie(int id);

    Task<IList<StatisticBase>> GetActorStatistic(DateTime fromDate);
    
    /// <summary>
    /// Updates a movie with the provided information.
    /// </summary>
    /// <param name="id">The ID of the movie to update.</param>
    /// <param name="userID">The ID of the user making the update.</param>
    /// <param name="movie">The updated movie information.</param>
    /// <returns>True if the update was successful, false otherwise.</returns>
    Task<bool> UpdateMovie(int id, int userID, Movie movie);
}
