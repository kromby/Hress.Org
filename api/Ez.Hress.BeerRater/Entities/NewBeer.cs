namespace Ez.Hress.BeerRater.Entities;

// The fields a user supplies when registering a new beer.
public class NewBeer
{
    public string? Name { get; set; }

    public string? Producer { get; set; }

    public string? Country { get; set; }

    public double? AlcoholPercentage { get; set; }

    public string? VinbudinUrl { get; set; }

    public string? Style { get; set; }

    public string? Description { get; set; }

    public int? ImageID { get; set; }
}
