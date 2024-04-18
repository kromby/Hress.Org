namespace Ez.Hress.Hardhead.Entities;

public class StatisticTextEntity : StatisticBase
{
    public StatisticTextEntity(string text)
    {
        Text = text;
    }
    public string Text { get; set; }
}
