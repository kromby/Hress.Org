using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Shared.UseCases;

public class VideoInteractor
{
    private readonly IVideoInfoDataAccess _videosDataAccess;
    private readonly IVideoContentDataAccess _contentDataAccess;
    private readonly ILogger<VideoInteractor> _log;

    public VideoInteractor(IVideoInfoDataAccess videoInfoDataAccess, IVideoContentDataAccess videoContentDataAccess, ILogger<VideoInteractor> log)
    {
        _videosDataAccess = videoInfoDataAccess;
        _contentDataAccess = videoContentDataAccess;
        _log = log;
    }

    public Task<VideoEntity?> GetVideoAsync(Guid id)
    {
        _log.LogInformation("[{Class}] Getting video: '{ID}'", nameof(VideoInteractor), id);
        return Task.FromResult(_videosDataAccess.GetVideo(id));
    }

    public async Task<VideoEntity?> GetContentAsync(Guid id)
    {
        _log.LogInformation("[{Class}] Getting content for video: '{ID}'", nameof(VideoInteractor), id);
        if (id == Guid.Empty)
        {
            _log.LogInformation("[{Class}] Invalid id: '{ID}'", nameof(VideoInteractor), id);
            throw new ArgumentNullException(nameof(id));
        }

        var video = await GetVideoAsync(id);
        if (video == null)
        {
            _log.LogInformation("[{Class}] Video not found: '{ID}'", nameof(VideoInteractor), id);
            return video;
        }

        string path = Path.Join("video", video.VideoUrl);
        // For videos, we typically don't want to load the entire content into memory
        // Instead, we'll populate metadata and provide streaming capabilities
        video.Content = await _contentDataAccess.GetContent(path);
        //video.FileSize = await _contentDataAccess.GetContentSize(path);
        //video.MimeType = await _contentDataAccess.GetContentType(path);

        return video;
    }

    //public async Task<Stream?> GetContentStreamAsync(int id)
    //{
    //    _log.LogInformation("[{Class}] Getting content stream for video: '{ID}'", nameof(VideoInteractor), id);
    //    if (id < 0)
    //    {
    //        _log.LogInformation("[{Class}] Invalid id: '{ID}'", nameof(VideoInteractor), id);
    //        throw new ArgumentNullException(nameof(id));
    //    }

    //    var video = await GetVideoAsync(id);
    //    if (video == null)
    //    {
    //        _log.LogInformation("[{Class}] Video not found: '{ID}'", nameof(VideoInteractor), id);
    //        return null;
    //    }

    //    string path = video.VideoUrl;
    //    IVideoContentDataAccess? contentDataAccess = _contentDataAccessList.FirstOrDefault(c => path.ToLowerInvariant().StartsWith(c.Prefix));

    //    if (contentDataAccess == null || string.IsNullOrWhiteSpace(contentDataAccess.Prefix))
    //    {
    //        _log.LogCritical("[{Class}] Invalid VideoUrl: '{Path}'", nameof(VideoInteractor), path);
    //        throw new SystemException("Invalid video URL prefix");
    //    }

    //    return await contentDataAccess.GetContentStream(path);
    //}

    //public async Task<VideoHrefEntity> SaveAsync(VideoEntity entity, int userID)
    //{
    //    if (entity == null)
    //        throw new ArgumentException("Can not be null", nameof(entity));

    //    entity.Validate();

    //    _log.LogInformation("[{Class}] VideoEntity: '{entity}'", nameof(VideoInteractor), entity);

    //    string? containerName = Enum.GetName(typeof(VideoContainer), entity.Container);
    //    if (containerName == null)
    //    {
    //        var ex = new ArgumentException($"Container '{entity.Container}' not found.");
    //        _log.LogError(ex, "Container not found");
    //        throw ex;
    //    }

    //    int typeID = 0;
    //    typeID = entity.Container switch
    //    {
    //        VideoContainer.Album => 30,
    //        VideoContainer.ATVR => 35,
    //        VideoContainer.News => 29,
    //        // Hardhead, Other
    //        _ => 36,
    //    };

    //    if (entity.ID == 0)
    //    {
    //        entity.Inserted = DateTime.UtcNow;
    //        entity.InsertedBy = userID;
    //        entity.VideoUrl = $"BLOB:/{containerName.ToLowerInvariant()}/@ID";
    //    }
    //    else
    //    {
    //        entity.Updated = DateTime.UtcNow;
    //        entity.UpdatedBy = userID;
    //    }

    //    IVideoContentDataAccess? contentDataAccess = _contentDataAccessList.FirstOrDefault(c => c.Prefix.Equals("blob:/"));
    //    if (contentDataAccess == null)
    //    {
    //        var ex = new SystemException(@"Data access with prefix 'blob:/' not found.");
    //        _log.LogError(ex, "Could not find correct content data access.");
    //        throw ex;
    //    }

    //    int id = 0;
    //    try
    //    {
    //        // For videos, we don't process the content like images
    //        // Instead, we rely on the metadata provided in the entity
    //        // In a real implementation, you might want to use a video processing library
    //        // to extract metadata from the video file

    //        id = await _videosDataAccess.Save(entity, typeID, entity.Width, entity.Height, 
    //            entity.Duration, entity.Codec, entity.FrameRate, entity.FileSize, entity.MimeType);
    //        _log.LogInformation("[{Class}] New video saved to metadata database: '{id}'", nameof(VideoInteractor), id);

    //        if (id == -1)
    //            throw new SystemException("Saving failed - unknown reason - ID -1");

    //        // Save the video content to blob storage
    //        if (entity.Content != null)
    //        {
    //            await contentDataAccess.Save(containerName.ToLowerInvariant(), entity.Content, id);
    //            _log.LogInformation("[{Class}] New video saved to blob store: '{id}'", nameof(VideoInteractor), id);
    //        }

    //        return new VideoHrefEntity(id, entity.Name);
    //    }
    //    catch (Exception ex)
    //    {
    //        _log.LogError(ex, "Error saving video");
    //        throw;
    //    }
    //}

    //public async Task<VideoHrefEntity> SaveStreamAsync(VideoEntity entity, Stream contentStream, string contentType, int userID)
    //{
    //    if (entity == null)
    //        throw new ArgumentException("Can not be null", nameof(entity));

    //    if (contentStream == null)
    //        throw new ArgumentException("Content stream can not be null", nameof(contentStream));

    //    entity.Validate();

    //    _log.LogInformation("[{Class}] VideoEntity: '{entity}'", nameof(VideoInteractor), entity);

    //    string? containerName = Enum.GetName(typeof(VideoContainer), entity.Container);
    //    if (containerName == null)
    //    {
    //        var ex = new ArgumentException($"Container '{entity.Container}' not found.");
    //        _log.LogError(ex, "Container not found");
    //        throw ex;
    //    }

    //    int typeID = 0;
    //    typeID = entity.Container switch
    //    {
    //        VideoContainer.Album => 30,
    //        VideoContainer.ATVR => 35,
    //        VideoContainer.News => 29,
    //        // Hardhead, Other
    //        _ => 36,
    //    };

    //    if (entity.ID == 0)
    //    {
    //        entity.Inserted = DateTime.UtcNow;
    //        entity.InsertedBy = userID;
    //        entity.VideoUrl = $"BLOB:/{containerName.ToLowerInvariant()}/@ID";
    //    }
    //    else
    //    {
    //        entity.Updated = DateTime.UtcNow;
    //        entity.UpdatedBy = userID;
    //    }

    //    IVideoContentDataAccess? contentDataAccess = _contentDataAccessList.FirstOrDefault(c => c.Prefix.Equals("blob:/"));
    //    if (contentDataAccess == null)
    //    {
    //        var ex = new SystemException(@"Data access with prefix 'blob:/' not found.");
    //        _log.LogError(ex, "Could not find correct content data access.");
    //        throw ex;
    //    }

    //    int id = 0;
    //    try
    //    {
    //        // Save metadata first
    //        id = await _videosDataAccess.Save(entity, typeID, entity.Width, entity.Height, 
    //            entity.Duration, entity.Codec, entity.FrameRate, entity.FileSize, contentType);
    //        _log.LogInformation("[{Class}] New video saved to metadata database: '{id}'", nameof(VideoInteractor), id);

    //        if (id == -1)
    //            throw new SystemException("Saving failed - unknown reason - ID -1");

    //        // Save the video content stream to blob storage
    //        await contentDataAccess.SaveStream(containerName.ToLowerInvariant(), contentStream, id, contentType);
    //        _log.LogInformation("[{Class}] New video saved to blob store: '{id}'", nameof(VideoInteractor), id);

    //        return new VideoHrefEntity(id, entity.Name);
    //    }
    //    catch (Exception ex)
    //    {
    //        _log.LogError(ex, "Error saving video stream");
    //        throw;
    //    }
    //}
} 