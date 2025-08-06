namespace Ez.Hress.Shared.Entities;

public class VideoEntity : EntityBase<Guid>
{
    public VideoEntity(Guid id, string name, string videoUrl)
    {
        ID = id;
        Name = name;
        VideoUrl = videoUrl;

        Validate();
    }

    public string VideoUrl { get; set; }

    public byte[]? Content { get; set; }

    // Video-specific properties
    public TimeSpan? Duration { get; set; }
    
    public string? Codec { get; set; }
    
    public int? FrameRate { get; set; }
    
    public int? Width { get; set; }
    
    public int? Height { get; set; }
    
    public long? FileSize { get; set; }
    
    public string? MimeType { get; set; }

    public void Validate()
    {
        if (ID == Guid.Empty)
            throw new ArgumentException("Can not be a empty.", nameof(ID));

        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Can not be null or empty.", nameof(Name));

        if (string.IsNullOrWhiteSpace(VideoUrl))
            throw new ArgumentException("Can not be null or empty.", nameof(VideoUrl));
    }
}