using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.Entities.InputModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public class HardheadParser
    {
        private readonly ILogger<HardheadParser> _log;

        public HardheadParser(ILogger<HardheadParser> log)
        {
            _log = log;
        }

        public MovieInfo ParseMovieInfo(MovieInfoModel movieInfoInput)
        {
            _log.LogInformation("Parsing movie information: {MovieInfoInput}", movieInfoInput);

            var movieInfo = new MovieInfo(movieInfoInput.Title, movieInfoInput.Plot, movieInfoInput.Rated, movieInfoInput.Country)
            {
                Year = int.Parse(movieInfoInput.Year),
                Released = DateTime.Parse(movieInfoInput.Released),
                Runtime = int.Parse(movieInfoInput.Runtime.Replace("min", "").Trim()),
                Genre = movieInfoInput.Genre.Split(", ").ToList(),
                Director = movieInfoInput.Director.Split(", ").ToList(),
                Writer = movieInfoInput.Writer.Split(", ").ToList(),
                Actors = movieInfoInput.Actors.Split(", ").ToList(),
                Description = movieInfoInput.Plot,
                Language = movieInfoInput.Language.Split(", ").ToList(),           
                Ratings = movieInfoInput.Ratings.ToDictionary(static r => r.Source, r => r.Value),
                Metascore = movieInfoInput.Metascore,
                ImdbRating = decimal.Parse(movieInfoInput.ImdbRating),
                ImdbVotes = int.Parse(movieInfoInput.ImdbVotes.Replace(",", "")),
                ImdbID = movieInfoInput.ImdbID,                
            };
            

            if(IsValidString(movieInfoInput.Awards))
            {
                movieInfo.Awards = movieInfoInput.Awards;
            }

            if(IsValidString(movieInfoInput.DVD))
            {
                movieInfo.DVDReleased = DateTime.Parse(movieInfoInput.DVD);
            }

            if (IsValidString(movieInfoInput.BoxOffice))
            {
                movieInfo.BoxOffice = movieInfoInput.BoxOffice;
            }

            if (IsValidString(movieInfoInput.Production))
            {
                movieInfo.Production = movieInfoInput.Production;
            }

            if (IsValidString(movieInfoInput.Website))
            {
                movieInfo.Website = movieInfoInput.Website;
            }

            return movieInfo;
        }

        private static bool IsValidString(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if(value == "N/A")
            {
                return false;
            }

            return true;
        }
    }
}
