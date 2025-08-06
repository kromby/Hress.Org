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
        return _videosDataAccess.GetVideoAsync(id);
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
} 