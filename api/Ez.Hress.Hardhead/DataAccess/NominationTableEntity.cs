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
            PartitionKey = String.Empty;
            RowKey = String.Empty;
            Description = String.Empty;
        }

        public NominationTableEntity(Nomination nomination)
        {
            PartitionKey = nomination.TypeID.ToString();
            RowKey = Guid.NewGuid().ToString();

            NomineeID = nomination.NomineeID;
            Description = nomination.Description;
            CreatedBy = nomination.CreatedBy;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public int NomineeID { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
    }
}
