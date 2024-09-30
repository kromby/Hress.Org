using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities.InputModels
{
    public class MovieInfoModel
    {
        public MovieInfoModel()
        {            
            Ratings = Array.Empty<RatingModel>();
            Title = string.Empty;
            Plot = string.Empty;
            Year = string.Empty;
            Released = string.Empty;
            Runtime = string.Empty;
            Genre = string.Empty;
            Director = string.Empty;
            Writer = string.Empty;
            Actors = string.Empty;
            Language = string.Empty;
            Country = string.Empty;
            Awards = string.Empty;
            Metascore = string.Empty;
            ImdbRating = string.Empty;
            ImdbVotes = string.Empty;
            ImdbID = string.Empty;
            DVD = string.Empty;
            BoxOffice = string.Empty;
            Production = string.Empty;
            Website = string.Empty;
        }

        public string Title { get; set; }
        public string Year { get; set; }
        public string? Rated { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string Writer { get; set; }
        public string Actors { get; set; }
        public string Plot { get; set; }
        public string Language { get; set; }
        public string Country { get; set; }
        public string Awards { get; set; }
        public IList<RatingModel> Ratings { get; set; }
        public string Metascore { get; set; }
        public string ImdbRating { get; set; }
        public string ImdbVotes { get; set; }
        public string ImdbID { get; set; }
        public string DVD { get; set; }
        public string BoxOffice { get; set; }
        public string Production { get; set; }
        public string Website { get; set; }
    }

    public class RatingModel
    {
        public string? Source { get; set; }
        public string? Value { get; set; }
    }
}
