using Ez.Hress.Hardhead.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public interface IHardheadStatisticsDataAccess
    {
        Task<IList<StatisticBase>> GetUserStatistic(DateTime fromDate, int attendanceTypeId);

        Task<IList<StatisticBase>> GetAttendanceStatistic(DateTime fromDate);
    }
}
