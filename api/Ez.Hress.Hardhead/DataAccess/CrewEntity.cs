using Azure;
using Azure.Data.Tables;
using Ez.Hress.Hardhead.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess
{
    internal class CrewEntity : ITableEntity
    {
        public CrewEntity()
        {
            PartitionKey = "Crew";
            RowKey = string.Empty;
        }

        public CrewEntity(CrewMember crew)
        {
            PartitionKey = "Crew";
            RowKey = crew.Name ?? "Missing name";
            MovieCounter = crew.MovieCounter;
        }

        public  int MovieCounter { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
