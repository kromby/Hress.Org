using Ez.Hress.Hardhead.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public interface IMovieDataAccess
    {
        /// <summary>
        /// Retrieves movies by a partial name.
        /// </summary>
        /// <param name="nameAndActorFilter">Partial name of film or actor.</param>
        /// <returns>A list of movies.</returns>
        Task<IList<Movie>> GetMovies(string nameAndActorFilter);

        Task<IList<StatisticBase>> GetActorStatistic(DateTime fromDate);
    }
}
