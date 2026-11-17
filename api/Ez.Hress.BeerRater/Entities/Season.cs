namespace Ez.Hress.BeerRater.Entities;

public class Season
{
    public string ID { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    // ID in the shared gen_Type table (SQL). Season types are not used in the MVP.
    public int? TypeID { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsOpen => EndDate == null;
}
