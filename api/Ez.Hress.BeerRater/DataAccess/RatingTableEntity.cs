using Azure;
using Azure.Data.Tables;
using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.DataAccess;

internal class RatingTableEntity : ITableEntity
{
    public RatingTableEntity()
    {
        PartitionKey = string.Empty;
        RowKey = string.Empty;
        BeerID = string.Empty;
    }

    public RatingTableEntity(Rating rating)
    {
        PartitionKey = Uri.EscapeDataString(rating.SeasonID);
        RowKey = Uri.EscapeDataString($"{rating.BeerID}_{rating.UserID}");
        BeerID = rating.BeerID;
        UserID = rating.UserID;
        Stars = rating.Stars;
        Created = DateTime.SpecifyKind(rating.Created, DateTimeKind.Utc);
        Updated = DateTime.SpecifyKind(rating.Updated, DateTimeKind.Utc);
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string BeerID { get; set; }
    public int UserID { get; set; }
    public int Stars { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    public Rating ToRating() => new()
    {
        SeasonID = Uri.UnescapeDataString(PartitionKey),
        BeerID = BeerID,
        UserID = UserID,
        Stars = Stars,
        Created = DateTime.SpecifyKind(Created, DateTimeKind.Utc),
        Updated = DateTime.SpecifyKind(Updated, DateTimeKind.Utc)
    };
}
