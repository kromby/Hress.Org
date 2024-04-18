using System.Globalization;

namespace Ez.Hress.Hardhead.Entities;

public class StatsEntity
{
    public StatsEntity(string typeName)
    {
        TypeName = typeName;
        List = new List<StatisticBase>();
    }

    public PeriodType PeriodType { get; set; }

    public string PeriodTypeName { get => Enum.GetName(typeof(PeriodType), this.PeriodType) ?? string.Empty; }

    public string TypeName { get; set; }
    public DateTime DateFrom { get; set; }
    public string DateFromString { get => this.DateFrom.ToString("yyyy", CultureInfo.GetCultureInfo("is-IS")); }

    public IList<StatisticBase> List { get; set; }
}
