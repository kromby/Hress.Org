using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.DataAccess;
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
    public class MovieSqlAccess : IMovieDataAccess
    {
        private readonly ILogger<MovieSqlAccess> _log;
        private readonly string _connectionString;
        private readonly string _class = nameof(MovieSqlAccess);

        private const string SQL_GETMOVIE = @"SELECT	night.Id, film.TextValue Movie, actor.TextValue Actor, poster.ImageId poster, 
                                                        imdb.TextValue imdb, why.TextValue why, youtube.TextValue youtube, CAST(moviekills.Count as int) moviekills, CAST(hhkills.Count as int) hhkills
                                                FROM    rep_Event night
                                                JOIN    rep_Text film ON film.EventId = night.Id AND film.TypeId = 62
                                                JOIN    rep_Text actor ON actor.EventId = night.Id AND actor.TypeId = 63
                                                LEFT OUTER JOIN    rep_Text imdb ON imdb.EventId = night.Id AND imdb.TypeId = 60
                                                LEFT OUTER JOIN rep_Text why ON why.EventId = night.Id AND why.TypeId = 59
                                                LEFT OUTER JOIN rep_Image poster ON poster.EventId = night.Id AND poster.TypeId = 14
                                                LEFT OUTER JOIN rep_Text youtube ON youtube.EventId = night.Id AND youtube.TypeId = 208
												LEFT OUTER JOIN rep_Count moviekills ON moviekills.EventId = night.Id AND moviekills.TypeId = 221
												LEFT OUTER JOIN rep_Count hhkills ON hhkills.EventId = night.Id AND hhkills.TypeId = 222
                                                WHERE   night.TypeId = 49";
        public MovieSqlAccess(DbConnectionInfo connectionInfo, ILogger<MovieSqlAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }

        public async Task<IList<Movie>> GetMovies(string nameAndActorFilter)
        {
            var sql = string.Format("{0} {1}", SQL_GETMOVIE, "AND (film.TextValue LIKE @filter OR actor.TextValue LIKE @filter)");
            _log.LogInformation("SQL: {SQL}", sql);

            var list = new List<Movie>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter("filter", string.Format("%{0}%", nameAndActorFilter)));

                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var entity = ParseMovie(reader);
                    list.Add(entity);
                }
            }

            return list;
        }

        private static Movie ParseMovie(SqlDataReader reader)
        {
            return new Movie()
            {
                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                Name = reader.GetString(reader.GetOrdinal("Movie")),
                Actor = reader.GetString(reader.GetOrdinal("Actor")),
                ImdbUrl = reader[reader.GetOrdinal("imdb")].ToString(),
                YoutubeUrl = reader[reader.GetOrdinal("youtube")].ToString(),
                Reason = reader[reader.GetOrdinal("why")].ToString(),
                PosterPhotoID = SqlHelper.GetNullableInt(reader, "poster"),
                MovieKillCount = SqlHelper.GetNullableInt(reader, "moviekills"),
                HardheadKillCount = SqlHelper.GetNullableInt(reader, "hhkills")
            };
        }

        public async Task<IList<StatisticBase>> GetActorStatistic(DateTime fromDate)
        {            
            var sql = @"SELECT	actor.TextValue 'Actor', COUNT(actor.Id) AttendedCount, MIN(hardhead.Date) FirstAttended, MAX(hardhead.Date) LastAttended
                        FROM	rep_Text actor
                        JOIN	rep_Event hardhead ON actor.EventId = hardhead.Id AND hardhead.TypeId = 49
                        WHERE	actor.TypeID = 63
	                        AND	hardhead.Date > @fromDate
                        GROUP BY actor.TextValue
                        ORDER BY COUNT(actor.Id) DESC";

            _log.LogInformation("[{Class}.{Method}] Executing SQL: '{SQL}'", _class, nameof(GetActorStatistic), sql);

            var list = new List<StatisticBase>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter("fromDate", fromDate));

                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var entity = new StatisticTextEntity(reader.GetString(reader.GetOrdinal("Actor")))
                    {
                        AttendedCount = reader.GetInt32(reader.GetOrdinal("AttendedCount")),
                        FirstAttended = reader.GetDateTime(reader.GetOrdinal("FirstAttended")),
                        LastAttended = reader.GetDateTime(reader.GetOrdinal("LastAttended"))
                    };

                    list.Add(entity);
                }
            }

            return list;
        }
    }
}
