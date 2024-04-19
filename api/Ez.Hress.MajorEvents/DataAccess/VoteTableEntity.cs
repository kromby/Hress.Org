using Ez.Hress.MajorEvents.Entities;
using Azure.Data.Tables;
using Azure;

namespace Ez.Hress.MajorEvents.DataAccess;

internal class VoteTableEntity : ITableEntity
{
    public VoteTableEntity()
    {
        PartitionKey = string.Empty;
        RowKey = string.Empty;
    }

    public VoteTableEntity(Vote vote)
    {
        PartitionKey = vote.TypeID.ToString();
        RowKey = vote.InsertedBy.ToString();

        CourseID = vote.CourseID;
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public int CourseID { get; set; }
}
