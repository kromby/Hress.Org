using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Hardhead.Entities;

public class Movie : EntityBase<int>
{
    public string? ImdbUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? Actor { get; set; }
    public string? Reason { get; set; }
    public int? MovieKillCount { get; set; }
    public int? HardheadKillCount { get; set; }
    public HrefEntity Hardhead
    {
        get => new()
{
Href = string.Format("/api/hardhead/{0}", ID)
};
    }

    public int? PosterPhotoID { private get; set; }
    public HrefEntity? PosterPhoto
    {
        get
        {
            if (PosterPhotoID.HasValue)
                return new HrefEntity()
                {
                    ID = PosterPhotoID.Value,
                    Href = string.Format("/api/images/{0}/content", PosterPhotoID.Value)
                };

            return null;
        }
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Can not be null or empty.", nameof(Name));
        if (string.IsNullOrWhiteSpace(Actor))
            throw new ArgumentException("Can not be null or empty.", nameof(Actor));

        if (!string.IsNullOrWhiteSpace(ImdbUrl) && !ImdbUrl.ToLower().Contains("imdb.com"))
            throw new ArgumentException("IMDB URL must contain \"imdb.com\".", nameof(ImdbUrl));
    }
}
