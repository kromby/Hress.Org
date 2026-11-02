namespace Ez.Hress.BeerRater.Entities;

public class Rating
{
    public string SeasonID { get; set; } = string.Empty;

    public string BeerID { get; set; } = string.Empty;

    public int UserID { get; set; }

    public int Stars { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }
}
