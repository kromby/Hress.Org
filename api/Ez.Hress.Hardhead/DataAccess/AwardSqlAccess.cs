using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Ez.Hress.Hardhead.DataAccess;

public class AwardSqlAccess : IAwardDataAccess
{
    private readonly ILogger<AwardSqlAccess> _log;
    private readonly string _connectionString;

    public AwardSqlAccess(DbConnectionInfo connectionInfo, ILogger<AwardSqlAccess> log)
    {
        _connectionString= connectionInfo.ConnectionString;
        _log = log;
    }

    public async Task<IList<Award>> GetAwards(int? year = null)
    {
        var sql = @"SELECT	evnt.Id, txt.TextValue, evnt.Date
                        FROM	rep_Event evnt
                        JOIN	rep_Text txt ON evnt.Id = txt.EventId AND txt.TypeId = 16
                        WHERE	evnt.TypeId = 74";
        
        _log.LogInformation("[{Class}] Executing SQL: '{SQL}'", nameof(GetAwards), sql);
        if (year.HasValue)
        {
            _log.LogInformation("[{Class}] userID: {userID}", nameof(GetAwards), year.Value);
        }

        IList<Award> list = new List<Award>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            if (year.HasValue)
            {
                command.CommandText = sql + @"	AND	evnt.Id IN
	                                                ( SELECT	winner.EventId
		                                              FROM	rep_Event ayear
		                                              JOIN	rep_User winner ON ayear.Id = winner.GroupId AND winner.TypeId = 54
		                                              WHERE	ayear.Id = @year AND	ayear.TypeId = 49
		                                              GROUP BY winner.EventId)";
                command.Parameters.Add(new SqlParameter("year", year.Value));
            }

            var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var award = new Award()
                {
                    ID = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("TextValue")),
                    Inserted = reader.GetDateTime(reader.GetOrdinal("Date"))
                };

                list.Add(award);
            }
        }
        return list;
    }

    public async Task<Award> GetAward(int id)
    {
        var sql = @"SELECT	evnt.Id, txt.TextValue, evnt.Date, grp.Id 'YearID', grp.Number 'YearNumber', COUNT(usr.id) 'VotedCount'
                        FROM	rep_Event evnt
                        JOIN	rep_Text txt ON evnt.Id = txt.EventId AND txt.TypeId = 16
                        JOIN	rep_User usr ON evnt.Id = usr.EventId
                        JOIN	rep_Event grp ON usr.GroupId = grp.Id
                        WHERE	evnt.Id = @id
                        GROUP BY evnt.Id, txt.TextValue, evnt.Date, grp.Id, grp.Number
                        ORDER BY grp.Id DESC";
        
        _log.LogInformation("[{Class}] Getting award with ID: '{ID}'", nameof(GetAward), id);

        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("id", id));

        var reader = await command.ExecuteReaderAsync();
        Award? award = null;

        while (await reader.ReadAsync())
        {
            if (award == null)
            {
                award = new Award
                {
                    ID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Inserted = reader.GetDateTime(2)
                };
            }

            var year = new YearEntity
            {
                ID = reader.GetInt32(3),          // YearID
                Name = reader.GetInt32(4).ToString(),  // YearNumber
                GuestCount = reader.GetInt32(5)   // VotedCount
            };

            award.Years.Add(year);
        }
        return award;
    }

    public async Task<IList<WinnerEntity>> GetAwardWinners(int awardId, int? year = null, int? position = null)
    {
        var sql = @"SELECT	winner.Id, winner.UserId, hressUser.Username, userPhoto.ImageId, winner.Position, 
                            eventYear.Number, winner.Value, winner.Text, winner.MeasurementTypeId, 
                            measurementType.Shortcode, measurementType.Name
                    FROM	rep_User winner
                    JOIN	rep_Event eventYear ON winner.GroupId = eventYear.Id
                    JOIN	adm_User hressUser ON winner.UserId = hressUser.Id
                    LEFT OUTER JOIN	upf_Image userPhoto ON hressUser.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                    LEFT OUTER JOIN	gen_Type measurementType ON winner.MeasurementTypeId = measurementType.Id
                    WHERE	winner.EventId = @awardId";

        if (year.HasValue)
        {
            sql += " AND eventYear.Id = @year";
        }

        if (position.HasValue)
        {
            sql += " AND winner.Position = @position";
        }

        sql += " ORDER BY eventYear.Number DESC, winner.Position";

        _log.LogInformation("[{Class}] Executing SQL: '{SQL}'", nameof(GetAwardWinners), sql);

        var winners = new List<WinnerEntity>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("awardId", awardId));

            if (year.HasValue)
            {
                command.Parameters.Add(new SqlParameter("year", year.Value));
            }

            if (position.HasValue)
            {
                command.Parameters.Add(new SqlParameter("position", position.Value));
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var winner = new WinnerEntity
                {
                    ID = reader.GetInt32(reader.GetOrdinal("Id")),
                    WinnerUserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Winner = new UserBasicEntity
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("UserId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        ProfilePhotoId = reader.IsDBNull(reader.GetOrdinal("ImageId")) ? 0 : reader.GetInt32(reader.GetOrdinal("ImageId"))
                    },
                    Position = reader.GetInt32(reader.GetOrdinal("Position")),
                    Year = reader.GetInt32(reader.GetOrdinal("Number")),
                    Value = reader.IsDBNull(reader.GetOrdinal("Value")) ? null : reader.GetDecimal(reader.GetOrdinal("Value")),
                    Text = reader.IsDBNull(reader.GetOrdinal("Text")) ? string.Empty : reader.GetString(reader.GetOrdinal("Text")),
                    MeasurementType = reader.IsDBNull(reader.GetOrdinal("MeasurementTypeId")) 
                        ? null 
                        : new TypeEntity(reader.GetInt32(reader.GetOrdinal("MeasurementTypeId")))
                        {
                            Code = reader.GetString(reader.GetOrdinal("Shortcode")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        }
                };

                winners.Add(winner);
            }
        }

        return winners;
    }
}
