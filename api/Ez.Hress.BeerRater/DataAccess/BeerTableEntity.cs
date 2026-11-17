using Azure;
using Azure.Data.Tables;
using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.DataAccess;

internal class BeerTableEntity : ITableEntity
{
    public const string Partition = "beer";

    public BeerTableEntity()
    {
        PartitionKey = Partition;
        RowKey = string.Empty;
        Name = string.Empty;
        Producer = string.Empty;
        Country = string.Empty;
    }

    public BeerTableEntity(Beer beer)
    {
        PartitionKey = Partition;
        RowKey = Uri.EscapeDataString(beer.ID);
        Name = beer.Name;
        Producer = beer.Producer;
        AlcoholPercentage = beer.AlcoholPercentage;
        Country = beer.Country;
        VinbudinUrl = beer.VinbudinUrl;
        Style = beer.Style;
        Description = beer.Description;
        ImageID = beer.ImageID;
        RegisteredBy = beer.RegisteredBy;
        RegisteredAt = DateTime.SpecifyKind(beer.RegisteredAt, DateTimeKind.Utc);
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Name { get; set; }
    public string Producer { get; set; }
    public double AlcoholPercentage { get; set; }
    public string Country { get; set; }
    public string? VinbudinUrl { get; set; }
    public string? Style { get; set; }
    public string? Description { get; set; }
    public int? ImageID { get; set; }
    public int RegisteredBy { get; set; }
    public DateTime RegisteredAt { get; set; }

    public static Beer ToBeer(TableEntity entity) => new()
    {
        ID = Uri.UnescapeDataString(entity.RowKey),
        Name = TableValues.GetString(entity, nameof(Name)),
        Producer = TableValues.GetString(entity, nameof(Producer)),
        AlcoholPercentage = TableValues.GetDouble(entity, nameof(AlcoholPercentage)),
        Country = TableValues.GetString(entity, nameof(Country)),
        VinbudinUrl = TableValues.GetOptionalString(entity, nameof(VinbudinUrl)),
        Style = TableValues.GetOptionalString(entity, nameof(Style)),
        Description = TableValues.GetOptionalString(entity, nameof(Description)),
        ImageID = TableValues.GetOptionalInt32(entity, nameof(ImageID)),
        RegisteredBy = TableValues.GetInt32(entity, nameof(RegisteredBy)),
        RegisteredAt = TableValues.GetDateTime(entity, nameof(RegisteredAt))
    };
}
