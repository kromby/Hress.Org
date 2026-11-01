namespace Ez.Hress.BeerRater.Entities;

public class Season
{
    public string ID { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Type { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsOpen => EndDate == null;
}
