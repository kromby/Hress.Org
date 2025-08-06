using Azure;
using Azure.Data.Tables;
using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.DataAccess;

internal class VideoTableEntity : ITableEntity
{
    public VideoTableEntity()
    {
        PartitionKey = "Default";
        RowKey = Guid.NewGuid().ToString();
        Name = string.Empty;
        VideoUrl = string.Empty;
    }

    public VideoTableEntity(VideoEntity entity)
    {
        PartitionKey = entity.ID.ToString();
        RowKey = entity.VideoUrl[..entity.VideoUrl.IndexOf('/')];
        Name = entity.Name ?? string.Empty;
        VideoUrl = entity.VideoUrl;
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Name { get; set; }
    public string VideoUrl { get; set; }
}