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
    public int RegisteredBy { get; set; }
    public DateTime RegisteredAt { get; set; }

    public Beer ToBeer() => new()
    {
        ID = Uri.UnescapeDataString(RowKey),
        Name = Name,
        Producer = Producer,
        AlcoholPercentage = AlcoholPercentage,
        Country = Country,
        VinbudinUrl = VinbudinUrl,
        RegisteredBy = RegisteredBy,
        RegisteredAt = DateTime.SpecifyKind(RegisteredAt, DateTimeKind.Utc)
    };
}
