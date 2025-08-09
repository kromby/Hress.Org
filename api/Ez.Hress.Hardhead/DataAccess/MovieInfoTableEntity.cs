using Azure;
using Azure.Data.Tables;
using Ez.Hress.Hardhead.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess;

internal class MovieInfoTableEntity : ITableEntity
{
    public MovieInfoTableEntity()
    {
        PartitionKey = string.Empty;
        RowKey = string.Empty;
        Rated = string.Empty;
        Description = string.Empty;
        Country = string.Empty;
        Awards = string.Empty;
        Genre = string.Empty;
        Language = string.Empty;
        Ratings = string.Empty;
    }

    public MovieInfoTableEntity(MovieInfo movieInfo)
    {
        PartitionKey = movieInfo.ID.ToString();
        RowKey = movieInfo.Name ?? "No Name";

        Year = movieInfo.Year;
        Rated = movieInfo.Rated;
        Released = DateTime.SpecifyKind(movieInfo.Released, DateTimeKind.Utc);
        Age = movieInfo.Age;
        Runtime = movieInfo.Runtime;        
        Country = movieInfo.Country;
        
        // Serialize collections for storage: Genre/Language as comma-separated values, Ratings as JSON
        Genre = movieInfo.Genre?.Any() == true 
            ? string.Join(", ", movieInfo.Genre) 
            : string.Empty;
        Language = movieInfo.Language?.Any() == true 
            ? string.Join(", ", movieInfo.Language) 
            : string.Empty;
        Ratings = movieInfo.Ratings?.Any() == true 
            ? System.Text.Json.JsonSerializer.Serialize(movieInfo.Ratings) 
            : string.Empty;



        Description = movieInfo.Description ?? string.Empty;

        Awards = movieInfo.Awards ?? string.Empty;

        if (movieInfo.DVDReleased != null)
        {
            DVDReleased = DateTime.SpecifyKind(movieInfo.DVDReleased.Value, DateTimeKind.Utc);
        }

        if(movieInfo.Metascore != null)
        {
            Metascore = movieInfo.Metascore;
        }

        if(movieInfo.ImdbID != null)
        {
            ImdbRating = movieInfo.ImdbRating;
            ImdbVotes = movieInfo.ImdbVotes;
            ImdbID = movieInfo.ImdbID;
        }

        if(movieInfo.BoxOffice != null)
        {
            BoxOffice = movieInfo.BoxOffice;
        }

        if(movieInfo.Production != null)
        {
            Production = movieInfo.Production;
        }

        if(movieInfo.Website != null)
        {
            Website = movieInfo.Website;
        }
    }

    public int Year { get; set; }
    public string Rated { get; set; }
    public DateTime Released { get; set; }
    public DateTime? DVDReleased { get; set; }
    public int Age { get; set; }
    public int Runtime { get; set; }
    public string Description { get; set; }
    public string Country { get; set; }
    public string Awards { get; set; }
    public string? Metascore { get; set; }
    public decimal ImdbRating { get; set; }
    public int ImdbVotes { get; set; }
    public string? ImdbID { get; set; }
    public string? BoxOffice { get; set; }
    public string? Production { get; set; }
    public string? Website { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Ratings { get; set; } = string.Empty;

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
