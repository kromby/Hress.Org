using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public interface IHardheadStatisticsDataAccess
{
    Task<IList<StatisticBase>> GetUserStatistic(DateTime fromDate, int attendanceTypeId);

    Task<IList<StatisticBase>> GetAttendanceStatistic(DateTime fromDate);
}
