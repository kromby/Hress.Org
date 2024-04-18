using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
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
}
