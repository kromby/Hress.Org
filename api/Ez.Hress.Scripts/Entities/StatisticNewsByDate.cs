using System.Globalization;

namespace Ez.Hress.Scripts.Entities;

public enum DatePart
{
    Day,
    Month,
    Year
}

public class StatisticNewsByDate
{
    public StatisticNewsByDate(DatePart unit, int value, int count)
    {
        Unit = unit;
        Value = value;
        Count = count;

        if (Unit == DatePart.Month)
        {
            var dt = new DateTime(2000, value, 1);
            ValueString = $"{dt.ToString("MMMM", CultureInfo.GetCultureInfo("is-IS"))} YEAR";
        }
        else
            ValueString = $"Árið {Value}";
    }
            
    public int Value { get; set; }
    public string ValueString { get; set; }
    public int Count { get; set; }
    public DatePart Unit { get; set; }
    public string UnitName { get => Enum.GetName(typeof(DatePart), Unit) ?? "Unknown"; }
}
