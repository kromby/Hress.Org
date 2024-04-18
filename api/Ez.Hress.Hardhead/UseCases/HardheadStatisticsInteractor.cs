using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Hardhead.UseCases;

public class HardheadStatisticsInteractor
{
    private readonly IHardheadStatisticsDataAccess _hardheadStatisticsDataAccess;
    private readonly IHardheadDataAccess _hardheadDataAccess;
    private readonly ITypeInteractor _typeInteractor;
    private readonly ILogger<HardheadStatisticsInteractor> _log;
    public HardheadStatisticsInteractor(IHardheadStatisticsDataAccess hardheadStatsDataAccess, IHardheadDataAccess hardheadDataAccess, ITypeInteractor typeInteractor, ILogger<HardheadStatisticsInteractor> log)
    {
        _hardheadStatisticsDataAccess = hardheadStatsDataAccess;
        _hardheadDataAccess = hardheadDataAccess;
        _typeInteractor = typeInteractor;
        _log = log;
    }

    public async Task<StatsEntity> GetUserStatisticsAsync(PeriodType periodType, int attendanceTypeID, int? id)
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

    public async Task<StatsEntity> GetAttendanceStatisticsAsync(PeriodType periodType)
    {
        _log.LogInformation("[{Class}] Getting attendance statistics for type: {PeriodType}", nameof(HardheadStatisticsInteractor), periodType);

        var entity = new StatsEntity("gestur")
        {
            PeriodType = periodType,
            DateFrom = Utility.GetDateFromPeriodType(periodType)
        };
        entity.List = await _hardheadStatisticsDataAccess.GetAttendanceStatistic(entity.DateFrom);

        return entity;
    }

    // TODO: get statistics about nominees and nominated by a specific user
    public async Task<object> GetChallangeHistoryAsync(int userID)
    {
        _log.LogInformation("[{Class}] Getting challenge history for user: {userID}", nameof(HardheadStatisticsInteractor), userID);

        var list = await _hardheadDataAccess.GetHardheads(new DateTime(1979, 9, 9), DateTime.MaxValue);

        var nextHardhead = new Dictionary<int, StatisticUserEntity>();
        var previousHardhead = new Dictionary<int, StatisticUserEntity>();


        HardheadNight? last = null;
        bool addChallenged = false;
        foreach (var night in list)
        {
            if(addChallenged && night.Host.ID != userID)
            {
                // skipcq: CS-P1005
                if (previousHardhead.ContainsKey(night.Host.ID))
                {
                    previousHardhead[night.Host.ID].AttendedCount++;
                    previousHardhead[night.Host.ID].FirstAttended = night.Date;
                }
                else
                {
                    previousHardhead.Add(night.Host.ID, new StatisticUserEntity()
                    {
                        AttendedCount = 1,
                        User = night.Host,
                        FirstAttended = night.Date,
                        LastAttended = night.Date,
                    });
                }

                addChallenged = false;
            }

            if(night.Host.ID == userID)
            {
                if (last != null && last.Host.ID != userID)
                {
                    // skipcq: CS-P1005
                    if (nextHardhead.ContainsKey(last.Host.ID))
                    {
                        nextHardhead[last.Host.ID].AttendedCount++;
                        nextHardhead[last.Host.ID].FirstAttended = last.Date;
                    }
                    else
                    {
                        nextHardhead.Add(last.Host.ID, new StatisticUserEntity()
                        {
                            AttendedCount = 1,
                            User = last.Host,
                            FirstAttended = last.Date,
                            LastAttended = last.Date,
                        });
                    }
                }

                addChallenged = true;
            }

            last = night;
        }

        return new ChallangeHistory(previousHardhead.Values.OrderByDescending(t => t.AttendedCount).ToList(), nextHardhead.Values.OrderByDescending(t => t.AttendedCount).ToList());
    }

    public async Task<IList<StatisticUserEntity>> GetStreaksAsync(int userID)
    {
        _log.LogInformation("[{Class}] Getting streaks of attended nights for user: {userID}", nameof(HardheadStatisticsInteractor), userID);

        var allNights = await _hardheadDataAccess.GetHardheadIDs(userID, null);

        var list = new List<StatisticUserEntity>();
        StatisticUserEntity? stat = null ;

        foreach(var night in allNights)
        {
            if(night.Host.ID == userID)
            {
                if(stat == null)
                {
                    stat = new StatisticUserEntity
                    {
                        FirstAttended = night.Date,
                        LastAttended = night.Date,
                        AttendedCount = 1,
                        User = new UserBasicEntity() { ID = userID },
                    };
                }
                else
                {
                    stat.AttendedCount++;
                    stat.LastAttended = night.Date;
                }
            }
            else
            {
                if (stat == null)
                    continue;

                if(stat.AttendedCount > 1)
                    list.Add(stat);

                stat = null;
            }
        }

        return list.OrderByDescending(s => s.AttendedCount).ToList();
    }
}
