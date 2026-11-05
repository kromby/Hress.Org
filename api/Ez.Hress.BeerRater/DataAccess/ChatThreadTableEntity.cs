using Azure;
using Azure.Data.Tables;

namespace Ez.Hress.BeerRater.DataAccess;

internal class ChatThreadTableEntity : ITableEntity
{
    public ChatThreadTableEntity()
    {
        PartitionKey = string.Empty;
        RowKey = string.Empty;
    }

    public ChatThreadTableEntity(string seasonID, string beerID, int threadID)
    {
        PartitionKey = Uri.EscapeDataString(seasonID);
        RowKey = Uri.EscapeDataString(beerID);
        ThreadID = threadID;
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // adm_Component ID of the beer's thread in the Hress chat.
    public int ThreadID { get; set; }
}
