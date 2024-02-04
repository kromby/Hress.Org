using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess
{
    public class HardheadStatisticSqlDataAccess : IHardheadStatisticsDataAccess
    {
        private readonly string _connectionString;
        private readonly ILogger<HardheadStatisticSqlDataAccess> _log;
        private readonly string _class;

        public HardheadStatisticSqlDataAccess(DbConnectionInfo connectionInfo, ILogger<HardheadStatisticSqlDataAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
            _class = nameof(HardheadStatisticSqlDataAccess);
        }

        public async Task<IList<StatisticBase>> GetUserStatistic(DateTime fromDate, int attendanceTypeId)
        {
            _log.LogInformation("[{Class}.{Method}] GetUserStatistic fromDate: '{fromDate}', typeID: {typeID}", _class, nameof(GetUserStatistic), fromDate, attendanceTypeId);
            string sql = @"SELECT	guest.UserId, COUNT(guest.Id) AttendedCount, MIN(hardhead.Date) FirstAttended, MAX(hardhead.Date) LastAttended, hUser.Username, userPhoto.ImageId
                                                        FROM	rep_User guest
                                                        JOIN	rep_Event hardhead ON guest.EventId = hardhead.Id AND hardhead.TypeId = 49
                                                        JOIN	adm_User hUser ON guest.UserId = hUser.Id
                                                        LEFT OUTER JOIN	upf_Image userPhoto ON hUser.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                                                        WHERE	{0}
	                                                        AND	hardhead.Date > @fromDate
                                                            AND	guest.UserId != 2646
                                                        GROUP BY guest.UserId, hUser.Username, userPhoto.ImageId
                                                        ORDER BY COUNT(guest.Id) DESC";            

            var list = new List<StatisticBase>();

            using (var connection = new SqlConnection(_connectionString))
            {
                if (attendanceTypeId > 52)
                    sql = string.Format(sql, "guest.TypeId = @guestTypeId");
                else
                    sql = string.Format(sql, "guest.TypeId IN (52, 53)");
                _log.LogInformation("[{Class}.{Method}] Executing SQL: '{SQL}'", _class, nameof(GetUserStatistic), sql);

                await connection.OpenAsync();
                using var command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter("fromDate", fromDate));
                if (attendanceTypeId > 0)
                    command.Parameters.AddWithValue("guestTypeId", attendanceTypeId);

                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var entity = new StatisticUserEntity
                    {
                        AttendedCount = reader.GetInt32(reader.GetOrdinal("AttendedCount")),
                        FirstAttended = reader.GetDateTime(reader.GetOrdinal("FirstAttended")),
                        LastAttended = reader.GetDateTime(reader.GetOrdinal("LastAttended")),
                        User = new UserBasicEntity()
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Username = reader.GetString(reader.GetOrdinal("Username"))
                        }
                    };

                    if (!reader.IsDBNull(reader.GetOrdinal("ImageId")))
                        entity.User.ProfilePhotoId = reader.GetInt32(reader.GetOrdinal("ImageId"));

                    list.Add(entity);
                }
            }

            return list;
        }
    }
}
