using Azure;
using Azure.Data.Tables;
using Ez.Hress.BeerRater.Entities;

namespace Ez.Hress.BeerRater.DataAccess;

internal class SeasonTableEntity : ITableEntity
{
    public const string Partition = "season";

    public SeasonTableEntity()
    {
        PartitionKey = Partition;
        RowKey = string.Empty;
        Name = string.Empty;
    }

    public SeasonTableEntity(Season season)
    {
        PartitionKey = Partition;
        RowKey = Uri.EscapeDataString(season.ID);
        Name = season.Name;
        TypeID = season.TypeID;
        StartDate = DateTime.SpecifyKind(season.StartDate, DateTimeKind.Utc);
        EndDate = season.EndDate.HasValue ? DateTime.SpecifyKind(season.EndDate.Value, DateTimeKind.Utc) : null;
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Name { get; set; }
    public int? TypeID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Season ToSeason() => new()
    {
        ID = Uri.UnescapeDataString(RowKey),
        Name = Name,
        TypeID = TypeID,
        StartDate = DateTime.SpecifyKind(StartDate, DateTimeKind.Utc),
        EndDate = EndDate.HasValue ? DateTime.SpecifyKind(EndDate.Value, DateTimeKind.Utc) : null
    };
}
