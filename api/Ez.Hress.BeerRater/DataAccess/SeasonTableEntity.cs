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

    public static Season ToSeason(TableEntity entity) => new()
    {
        ID = Uri.UnescapeDataString(entity.RowKey),
        Name = TableValues.GetString(entity, nameof(Name)),
        TypeID = TableValues.GetOptionalInt32(entity, nameof(TypeID)),
        StartDate = TableValues.GetDateTime(entity, nameof(StartDate)),
        EndDate = TableValues.GetOptionalDateTime(entity, nameof(EndDate))
    };
}
