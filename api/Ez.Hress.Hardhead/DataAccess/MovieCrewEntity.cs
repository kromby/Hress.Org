using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess;

internal class MovieCrewEntity : ITableEntity
{
    public MovieCrewEntity()
    {
        PartitionKey = string.Empty;
        RowKey = string.Empty;
        CrewMember = string.Empty;
        Role = string.Empty;
    }

    public MovieCrewEntity(string movieKey, string crewKey, string role)
    {
        PartitionKey = movieKey;
        RowKey = Guid.NewGuid().ToString();
        CrewMember = crewKey;
        Role = role;
    }

    public string CrewMember { get; set; }
    public string Role { get; set; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}