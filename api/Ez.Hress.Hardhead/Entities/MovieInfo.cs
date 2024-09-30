using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities
{
    public class MovieInfo : EntityBase<int>
    {
        public MovieInfo(string title, string plot, string rated, string country)
        {
            Name = title;
            Description = plot;
            Rated = rated;
            Country = country;
            Inserted = DateTime.UtcNow; 
            Genre = new List<string>();
            Director = new List<string>();
            Writer = new List<string>();
            Actors = new List<string>();
            Language = new List<string>();
            Ratings = new Dictionary<string, string>();
        }

        public int Year { get; set; }
        public string Rated { get; set; }
        public DateTime Released { get; set; }
        public DateTime? DVDReleased { get; set; }
        public int Age { get; set; }    
        public int Runtime { get; set; }
        public IList<string> Genre { get; set; }
        public IList<string> Director { get; set; }
        public IList<string> Writer { get; set; }
        public IList<string> Actors { get; set; }
        public IList<string> Language { get; set; }
        public string Country { get; set; }
        public string? Awards { get; set; }
        public Dictionary<string, string> Ratings { get; set; }
        public string? Metascore { get; set; }
        public decimal ImdbRating { get; set; }
        public int ImdbVotes { get; set; }
        public string? ImdbID { get; set; }
        public string? BoxOffice { get; set; }
        public string? Production { get; set; }
        public string? Website { get; set; }
    }
}
