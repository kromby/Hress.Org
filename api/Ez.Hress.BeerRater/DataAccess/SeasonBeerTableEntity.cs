using Azure;
using Azure.Data.Tables;

namespace Ez.Hress.BeerRater.DataAccess;

internal class SeasonBeerTableEntity : ITableEntity
{
    public SeasonBeerTableEntity()
    {
        PartitionKey = string.Empty;
        RowKey = string.Empty;
    }

    public SeasonBeerTableEntity(string seasonID, string beerID)
    {
        PartitionKey = Uri.EscapeDataString(seasonID);
        RowKey = Uri.EscapeDataString(beerID);
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
