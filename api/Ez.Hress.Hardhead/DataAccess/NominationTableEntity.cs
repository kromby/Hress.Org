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
    internal class NominationTableEntity : ITableEntity
    {
        public NominationTableEntity()
        {
            PartitionKey = string.Empty;
            RowKey = string.Empty;
            Description = string.Empty;
            NomineeName = string.Empty;
        }

        public NominationTableEntity(Nomination nomination)
        {
            PartitionKey = nomination.TypeID.ToString();
            RowKey = Guid.NewGuid().ToString();

            NomineeID = nomination.Nominee.ID;
            NomineeName = nomination.Nominee.Name ?? string.Empty;
            Description = nomination.Description;
            CreatedBy = nomination.CreatedBy;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public int NomineeID { get; set; }
        public string NomineeName { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
    }
}
