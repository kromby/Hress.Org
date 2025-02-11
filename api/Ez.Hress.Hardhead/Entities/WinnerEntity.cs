using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Hardhead.Entities;

public class WinnerEntity : EntityBase<int>
{
    public WinnerEntity()
    {
        Winner = new UserBasicEntity();
        Text = string.Empty;
    }

    public int WinnerUserId { get; set; }

    public UserBasicEntity Winner { get; set; }

    public int Position { get; set; }

    public int Year { get; set; }

    public decimal? Value { get; set; }

    public TypeEntity? MeasurementType { get; set; }

    public string Text { get; set; }
}
