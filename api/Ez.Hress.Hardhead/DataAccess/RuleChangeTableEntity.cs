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
    internal class RuleChangeTableEntity : ITableEntity
    {
        public RuleChangeTableEntity()
        {
            PartitionKey = string.Empty;
            RowKey = string.Empty;
            RuleText = string.Empty;
        }

        public RuleChangeTableEntity(RuleChange change)
        {
            PartitionKey = change.TypeID.ToString();
            RowKey = Guid.NewGuid().ToString();

            RuleCategoryID = change.RuleCategoryID;
            RuleText = change.RuleText;
            CreatedBy = change.InsertedBy;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public int RuleCategoryID { get; set; }
        public string? RuleText { get; set; }

        public int CreatedBy { get; set; }
    }
}
