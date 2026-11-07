namespace Ez.Hress.BeerRater.Entities;

public class Beer
{
    public string ID { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Producer { get; set; } = string.Empty;

    public double AlcoholPercentage { get; set; }

    public string Country { get; set; } = string.Empty;

    public string? VinbudinUrl { get; set; }

    // The beer's style, e.g. "Lager" or "Bock".
    public string? Style { get; set; }

    public string? Description { get; set; }

    // The beer's cover photo, an image in the hress.org image store.
    public int? ImageID { get; set; }

    public int RegisteredBy { get; set; }

    public DateTime RegisteredAt { get; set; }
}
