using Azure.Data.Tables;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Shared.DataAccess;

public class VideoInfoTableAccess(BlobConnectionInfo connectionInfo, ILogger<VideoInfoTableAccess> log) : IVideoInfoDataAccess
{
    private readonly TableClient _tableClient = new (connectionInfo.ConnectionString, "Videos");

    public async Task<VideoEntity?> GetVideoAsync(Guid id)
    {
        log.LogInformation("[{Class}] GetVideoAsync", this.GetType().Name);
        var result = _tableClient.QueryAsync<VideoTableEntity>(v => v.RowKey == id.ToString());

        var resultList = new List<VideoTableEntity>();
        await foreach (var entity in result)
        {
            resultList.Add(entity);
            if (resultList.Count >= 2) break; // Take at most 2 items
        }

        if(resultList.Count != 1)
            return null;

        var videoEntity = resultList.First();
        var video = new VideoEntity(Guid.Parse(videoEntity.RowKey), videoEntity.Name, $"{videoEntity.PartitionKey}/{videoEntity.VideoUrl}");

       return video;
    }
}