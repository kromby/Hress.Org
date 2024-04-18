using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public enum UserType
{
    host,
    guest,
    all
}

internal class Utility
{
    public static DateTime GetDateFromPeriodType(PeriodType periodType)
    {
        DateTime tempDate = new(2000, 1, 1);
        if (periodType == PeriodType.Last10)
        {
            tempDate = new DateTime(DateTime.Today.AddYears(-9).Year, 1, 1);
        }
        else if (periodType == PeriodType.Last5)
        {
            tempDate = new DateTime(DateTime.Today.AddYears(-5).Year, 1, 1);
        }
        else if (periodType == PeriodType.Last2)
        {
            tempDate = new DateTime(DateTime.Today.AddYears(-2).Year, 1, 1);
        }
        else if (periodType == PeriodType.ThisYear)
        {
            tempDate = new DateTime(DateTime.Today.Year, 1, 1);
        }

        return tempDate;
    }
}
