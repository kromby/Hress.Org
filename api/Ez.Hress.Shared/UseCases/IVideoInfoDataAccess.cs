using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases;

public interface IVideoInfoDataAccess
{
    VideoEntity? GetVideo(Guid id);
    
    //Task<int> Save(VideoEntity entity, int typeID, int? width, int? height, TimeSpan? duration, string? codec, int? frameRate, long? fileSize, string? mimeType);
} 