using Ez.Hress.Hardhead.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public class MovieInteractor
    {
        private readonly IMovieDataAccess _movieDataAccess;
        private readonly ILogger<MovieInteractor> _log;

        public MovieInteractor(IMovieDataAccess movieDataAccess, ILogger<MovieInteractor> log)
        {
            _movieDataAccess = movieDataAccess;
            _log = log;
        }

        public Task<IList<Movie>> GetMovies(string filterBy)
        {
            _log.LogInformation("Getting movies by filter: {Filter}", filterBy);
            return _movieDataAccess.GetMovies(filterBy);
        }
    }
}
