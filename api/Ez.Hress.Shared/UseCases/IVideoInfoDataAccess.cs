using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases;

public interface IVideoInfoDataAccess
{
    Task<VideoEntity?> GetVideoAsync(Guid id);
    
    //Task<int> Save(VideoEntity entity, int typeID, int? width, int? height, TimeSpan? duration, string? codec, int? frameRate, long? fileSize, string? mimeType);
} 