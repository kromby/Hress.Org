using Azure.Data.Tables;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Shared.DataAccess;

public class VideoInfoTableAccess(BlobConnectionInfo connectionInfo, ILogger<VideoInfoTableAccess> log) : IVideoInfoDataAccess
{
    private readonly TableClient _tableClient = new TableClient(connectionInfo.ConnectionString, "Videos");

    public VideoEntity? GetVideo(Guid id)
    {
        log.LogInformation("[{Class}] GetVideo", this.GetType().Name);
        var result = _tableClient.Query<VideoTableEntity>(v => v.RowKey == id.ToString());

        if(result.Count() != 1)
            return null;

        var entity = result.First();
            var video = new VideoEntity(Guid.Parse(entity.RowKey), entity.Name, Path.Join(entity.PartitionKey, entity.VideoUrl));

       return video;
    }
}