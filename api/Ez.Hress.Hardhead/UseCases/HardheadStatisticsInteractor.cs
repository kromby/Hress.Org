using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public class HardheadStatisticsInteractor
    {
        private readonly IHardheadStatisticsDataAccess _hardheadStatisticsDataAccess;
        private readonly ITypeInteractor _typeInteractor;
        private readonly ILogger<HardheadStatisticsInteractor> _log;
        public HardheadStatisticsInteractor(IHardheadStatisticsDataAccess hardheadStatsDataAccess, ITypeInteractor typeInteractor, ILogger<HardheadStatisticsInteractor> log)
        {
            _hardheadStatisticsDataAccess = hardheadStatsDataAccess;
            _typeInteractor = typeInteractor;
            _log = log;
        }

        public async Task<StatsEntity> GetUserStatistics(PeriodType periodType, int attendanceTypeID, int? id)
        {
            _log.LogInformation("[{Class}] Getting user statistics for type: {PeriodType}", nameof(HardheadStatisticsInteractor), periodType);

            attendanceTypeID = attendanceTypeID == 0 ? 52 : attendanceTypeID;
            var attendanceType = await _typeInteractor.GetEzType(attendanceTypeID);

            var entity = new StatsEntity(attendanceType.Name?.ToLower() ?? "gestur")
            {
                PeriodType = periodType,
                DateFrom = Utility.GetDateFromPeriodType(periodType)
            };
            entity.List = await _hardheadStatisticsDataAccess.GetUserStatistic(entity.DateFrom, attendanceTypeID);

            if (id.HasValue)
                entity.List = entity.List.Where(x =>
                {
                    var xUser = (x as StatisticUserEntity);
                    if (xUser == null)
                        return false;
                    return xUser.User.ID == id.Value;
                }).ToList();

            return entity;
        }
    }
}
