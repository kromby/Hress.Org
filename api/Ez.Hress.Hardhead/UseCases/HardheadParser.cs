using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.Entities.InputModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases;

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

        var movieInfo = new MovieInfo(movieInfoInput.Title, movieInfoInput.Plot, movieInfoInput.Rated ?? string.Empty, movieInfoInput.Country)
        {
            Released = DateTime.Parse(movieInfoInput.Released),
            Genre = movieInfoInput.Genre.Split(", ").ToList(),
            Description = movieInfoInput.Plot,
            Language = movieInfoInput.Language.Split(", ").ToList(),           
            Ratings = movieInfoInput.Ratings.ToDictionary(static r => r.Source, r => r.Value),
            Metascore = movieInfoInput.Metascore,
            ImdbRating = decimal.Parse(movieInfoInput.ImdbRating),
            ImdbID = movieInfoInput.ImdbID,                
        };

        foreach (var director in movieInfoInput.Director.Split(","))
        {
            movieInfo.Crew.Add(new CrewMember { Name = director.Trim(), Role = Role.Director });
        }

        foreach (var writer in movieInfoInput.Writer.Split(","))
        {
            movieInfo.Crew.Add(new CrewMember { Name = writer.Trim(), Role = Role.Writer });
        }

        foreach (var actor in movieInfoInput.Actors.Split(","))
        {
            movieInfo.Crew.Add(new CrewMember { Name = actor.Trim(), Role = Role.Actor });
        }

        if (int.TryParse(movieInfoInput.Year, out int tempYear))
        {
            movieInfo.Year = tempYear;
        }

        if(int.TryParse(movieInfoInput.Runtime.Replace("min", "").Trim(), out int tempRuntime))
        {
            movieInfo.Runtime = tempRuntime;
        }

        if (int.TryParse(movieInfoInput.ImdbVotes.Replace(",", ""), out int tempImdbVote))
        {
            movieInfo.ImdbVotes = tempImdbVote;
        }



        if (IsValidString(movieInfoInput.Awards))
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
