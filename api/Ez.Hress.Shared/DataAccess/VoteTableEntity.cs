using Azure;
using Azure.Data.Tables;
using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.DataAccess
{
    internal class VoteTableEntity : ITableEntity
    {
        public VoteTableEntity()
        {
            PartitionKey = string.Empty;
            RowKey = Guid.NewGuid().ToString();

            CategoryID = string.Empty;
            Value = string.Empty;
        }

        public VoteTableEntity(VoteEntity vote)
        {
            PartitionKey = vote.StepID.ToString();
            RowKey = Guid.NewGuid().ToString();
            
            CategoryID = vote.ID.ToString();
            Value = vote.Value.ToString();
        }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Value { get; set; }

        public string CategoryID { get; set; }
    }
}
