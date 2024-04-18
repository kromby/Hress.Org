using Azure;
using Azure.Data.Tables;
using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.DataAccess;

internal class RuleChangeTableEntity : ITableEntity
{
    public RuleChangeTableEntity()
    {
        PartitionKey = string.Empty;
        RowKey = string.Empty;
        RuleText = string.Empty;
        Reasoning = string.Empty;
        Name = string.Empty;
    }

    public RuleChangeTableEntity(RuleChange change)
    {
        PartitionKey = change.TypeID.ToString();
        RowKey = Guid.NewGuid().ToString();

        RuleCategoryID = change.RuleCategoryID;
        RuleID = change.RuleID;
        RuleText = change.RuleText;
        Reasoning = change.Reasoning;
        CreatedBy = change.InsertedBy;
        Name = change.Name ?? "";
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Name { get; set; }
    public int RuleCategoryID { get; set; }
    public int? RuleID { get; set; }
    public string? RuleText { get; set; }
    public string Reasoning { get; set; }

    public int CreatedBy { get; set; }
}
