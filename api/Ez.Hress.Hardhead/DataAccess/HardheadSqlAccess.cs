using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Ez.Hress.Hardhead.DataAccess;

public class HardheadSqlAccess : IHardheadDataAccess
{
    private readonly ILogger<HardheadSqlAccess> _log;
    private readonly string _connectionString;

    private const string SQL_GETHARDHEAD = @"
        SELECT	night.Id, night.Number, night.Date, host.UserId, summary.TextValue 'Description', hressUser.Username, 
		        userPhoto.ImageId, COUNT(guest.ID) 'GuestCount', night.ParentId 'YearID', 
		        film.TextValue Movie, actor.TextValue Actor, poster.ImageId poster, 
                imdb.TextValue 'imdb', why.TextValue 'why', youtube.TextValue youtube, 
		        CAST(moviekills.Count as int) moviekills, CAST(hhkills.Count as int) hhkills
        FROM    rep_Event night
        JOIN	rep_User host ON host.EventId = night.Id AND host.TypeId = 53
        JOIN	adm_User hressUser ON host.UserId = hressUser.Id
        JOIN	rep_User guest ON guest.EventId = night.Id
        LEFT OUTER JOIN	upf_Image userPhoto ON hressUser.Id = userPhoto.UserId AND userPhoto.TypeId = 14
        LEFT OUTER JOIN	rep_Text summary ON summary.EventId = night.Id AND summary.TypeId = 37
        LEFT OUTER JOIN rep_Text film ON film.EventId = night.Id AND film.TypeId = 62
        LEFT OUTER JOIN rep_Text actor ON actor.EventId = night.Id AND actor.TypeId = 63
        LEFT OUTER JOIN rep_Text imdb ON imdb.EventId = night.Id AND imdb.TypeId = 60
        LEFT OUTER JOIN rep_Text why ON why.EventId = night.Id AND why.TypeId = 59
        LEFT OUTER JOIN rep_Image poster ON poster.EventId = night.Id AND poster.TypeId = 14
        LEFT OUTER JOIN rep_Text youtube ON youtube.EventId = night.Id AND youtube.TypeId = 208
        LEFT OUTER JOIN rep_Count moviekills ON moviekills.EventId = night.Id AND moviekills.TypeId = 221
        LEFT OUTER JOIN rep_Count hhkills ON hhkills.EventId = night.Id AND hhkills.TypeId = 222
        WHERE	night.TypeId = 49 AND {0}
        GROUP BY	night.Id, night.Number, night.Date, host.UserId, summary.TextValue, hressUser.Username, 
			        userPhoto.ImageId, night.ParentId, film.TextValue, actor.TextValue, poster.ImageId, 
			        imdb.TextValue, why.TextValue, youtube.TextValue, moviekills.Count, hhkills.Count
        ORDER BY night.Date DESC";

    public HardheadSqlAccess(DbConnectionInfo connectionInfo, ILogger<HardheadSqlAccess> log)
    {
        _connectionString = connectionInfo.ConnectionString;
        _log = log;
    }

    public async Task<IList<HardheadNight>> GetHardheads(DateTime fromDate, DateTime toDate)
    {
        _log.LogInformation("[{Class}] Getting hardhead between dates '{From}' and '{To}'.", nameof(HardheadSqlAccess), fromDate, toDate);
        var sql = string.Format(SQL_GETHARDHEAD, "night.Date > @dateFrom AND night.Date <= @dateTo");
        _log.LogInformation("[{Class}] SQL '{SQL}'.", nameof(HardheadSqlAccess), sql);

        IList<HardheadNight> list = new List<HardheadNight>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("typeId", 49));
            command.Parameters.Add(new SqlParameter("dateFrom", fromDate));
            command.Parameters.Add(new SqlParameter("dateTo", toDate));

            var reader = await command.ExecuteReaderAsync();
            ParseHardhead(list, reader);
        }
        return list;
    }

    public async Task<IList<HardheadNight>> GetHardheads(int parentID)
    {
        var sql = string.Format(SQL_GETHARDHEAD, "night.ParentId = @parentID");

        IList<HardheadNight> list = new List<HardheadNight>();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("typeId", 49));
            command.Parameters.Add(new SqlParameter("parentID", parentID));

            var reader = await command.ExecuteReaderAsync();
            ParseHardhead(list, reader);
        }
        return list;
    }

    public async Task<IList<HardheadNight>> GetHardheadsByMovieFilterAsync(string nameAndActorFilter)
    {
        var sql = string.Format(SQL_GETHARDHEAD, "(film.TextValue LIKE @filter OR actor.TextValue LIKE @filter)");
        _log.LogInformation("SQL: {SQL}", sql);

        IList<HardheadNight> list = new List<HardheadNight>();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("filter", string.Format("%{0}%", nameAndActorFilter)));

            var reader = await command.ExecuteReaderAsync();
            ParseHardhead(list, reader);
        }
        return list;
    }

    private static void ParseHardhead(IList<HardheadNight> list, SqlDataReader reader)
    {
        while (reader.Read())
        {
            var hardhead = new HardheadNight(Convert.ToInt32(reader["Id"]),
                Convert.ToInt32(reader["Number"]),
                new UserBasicEntity()
                {
                    ID = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Username = reader.GetString(reader.GetOrdinal("Username"))
                })
            {
                Date = Convert.ToDateTime(reader["Date"]),
                Description = reader["Description"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("Description")) : null,
                GuestCount = reader.GetInt32(reader.GetOrdinal("GuestCount")),
            };

            if (!reader.IsDBNull(reader.GetOrdinal("ImageId")))
                hardhead.Host.ProfilePhotoId = reader.GetInt32(reader.GetOrdinal("ImageId"));

            if (!reader.IsDBNull(reader.GetOrdinal("YearID")))
                hardhead.YearID = reader.GetInt32(reader.GetOrdinal("YearID"));

            if(!reader.IsDBNull(reader.GetOrdinal("Movie")))
            {
                hardhead.Movie = ParseMovie(reader);
            }

            list.Add(hardhead);
        }
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

    public async Task<IList<HardheadUser>> GetHardheadUsers(int yearID)
    {
        var sql = @"SELECT	usr.Id, usr.Username, usr.Inserted, uimg.ImageId, COUNT(usr.Id) attended
                        FROM rep_Event hh
                        JOIN    rep_User hhUsr ON hh.Id = hhUsr.EventId
                        JOIN adm_User usr ON hhUsr.UserId = usr.Id
                        LEFT JOIN   upf_Image uimg ON usr.Id = uimg.UserId AND uimg.TypeId = 14
                        WHERE hh.TypeId = 49
                            AND hh.ParentId = @yearID
                        GROUP BY usr.Id, usr.Username, usr.Inserted, uimg.ImageId
                        ORDER BY usr.Username";

        _log.LogInformation("[{Class}] userID: {yearID}", nameof(GetHardheadUsers), yearID);
        _log.LogInformation("[{Class}] Executing SQL: '{SQL}'", nameof(GetHardheadUsers), sql);

        var userList = new List<HardheadUser>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("yearID", yearID);

            var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var user = new HardheadUser()
                {
                    ID = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Username")),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    Inserted = reader.GetDateTime(reader.GetOrdinal("Inserted")),
                    Attended = reader.GetInt32(reader.GetOrdinal("attended"))
                };

                if (!reader.IsDBNull(reader.GetOrdinal("ImageId")))
                {
                    user.ProfilePhotoId = reader.GetInt32(reader.GetOrdinal("ImageId"));
                }

                userList.Add(user);
            }
        }

        return userList;
    }

    public async Task<IList<HardheadNight>> GetHardheads(IList<HardheadNight> idNights)
    {
        var sql = string.Format(SQL_GETHARDHEAD, "night.Id IN ({0})");

        var list = new List<HardheadNight>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using var command = new SqlCommand(sql, connection);

            var sb = new StringBuilder();
            int i = 0;
            foreach (var idNight in idNights)
            {
                var param = string.Format("@id{0}", i++);
                sb.Append(param);
                sb.Append(", ");
                command.Parameters.AddWithValue(param, idNight.ID);
            }

            command.CommandText = string.Format(sql, sb.ToString().Trim(new [] { ' ', ',' }));

            var reader = await command.ExecuteReaderAsync();
            ParseHardhead(list, reader);
        }
        return list;
    }

    public async Task<IList<HardheadNight>> GetHardheadIDs(int userID, UserType? type)
    {
        //var sql = @"SELECT	hardhead.EventId
        //            FROM	rep_User hardhead
        //            JOIN	rep_Event night ON night.TypeId = 49 AND night.Id = hardhead.EventId
        //            WHERE	hardhead.UserId = @userID";

        var sql = @"SELECT	hh.Id, hh.Number, hh.Date, usr.UserId
                        FROM	rep_Event hh
                        LEFT OUTER JOIN	rep_User usr ON hh.Id = usr.EventId AND usr.UserId = @userID
                        WHERE	hh.TypeId = 49 AND hh.ParentId IS NOT NULL";

        if (type == UserType.guest)
        {
            sql += " AND usr.TypeId = 52 ORDER BY hh.Number";
        }
        else if (type == UserType.host)
        {
            sql += " AND usr.TypeId = 53 ORDER BY hh.Number";
        }

        var list = new List<HardheadNight>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("userID", userID));

            var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var nightUserID = SqlHelper.GetNullableInt(reader, "UserId");
                HardheadNight night = new(SqlHelper.GetInt(reader, "Id"), SqlHelper.GetInt(reader, "Number"), new() { ID = nightUserID ?? 0 })
                {
                    Date = SqlHelper.GetDateTime(reader, "Date"),
                };
                list.Add(night);
            }
        }
        return list;
    }

    public async Task<HardheadNight> GetHardhead(int id)
    {
        var sql = string.Format(SQL_GETHARDHEAD, "night.Id = @ID");

        var list = new List<HardheadNight>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("typeId", 49));
            command.Parameters.Add(new SqlParameter("ID", id));

            var reader = await command.ExecuteReaderAsync();
            ParseHardhead(list, reader);
        }
        return list.Count == 1 ? list.First() : new HardheadNight(0, 0, new UserBasicEntity());
    }

    public async Task<bool> AlterHardhead(HardheadNight hardhead)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("UpdateHardhead", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("hhID", hardhead.ID);
        command.Parameters.AddWithValue("hhDate", hardhead.Date);
        command.Parameters.AddWithValue("Description", hardhead.Description ?? string.Empty);
        command.Parameters.AddWithValue("updatedDate", hardhead.Updated);
        command.Parameters.AddWithValue("updatedBy", hardhead.UpdatedBy);

        var affected = await command.ExecuteNonQueryAsync();

        if (affected == 1)
            return true;

        return false;
    }

    public async Task<bool> CreateHardhead(int hostID, DateTime nextDate, int currentUserID, DateTime changeDate)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("CreateHardhead", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("hostID", hostID);
        command.Parameters.AddWithValue("hhDate", nextDate);
        command.Parameters.AddWithValue("createdDate", changeDate);
        command.Parameters.AddWithValue("createdBy", currentUserID);

        var affected = await command.ExecuteNonQueryAsync();

        if (affected == 1)
            return true;

        return false;
    }

    public Task<IList<ComponentEntity>> GetActions(int hardheadID)
    {
        IList<ComponentEntity> list = new List<ComponentEntity>
        {
            new()
            {
                ID = 9000,
                Name = "Breyta",
                Description = "Settu inn nýjar upplýsingar um harðhausakvöldið",
                Link = new HrefEntity() { ID = 9000, Href = string.Format("/hardhead/{0}/edit", hardheadID) }
            }
        };
        return Task.Run(() => list);
    }

    public async Task<IList<UserBasicEntity>> GetGuests(int hardheadID)
    {
        var sql = @"SELECT	guest.UserId, hressUser.Username, userPhoto.ImageId
                        FROM	rep_User guest
                        JOIN	adm_User hressUser ON guest.UserId = hressUser.Id
                        LEFT OUTER JOIN	upf_Image userPhoto ON hressUser.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                        WHERE   guest.EventId = @id
                            AND guest.TypeId = 52
                        ORDER BY hressUser.Username";

        var list = new List<UserBasicEntity>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("id", hardheadID));

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var href = new UserBasicEntity()
                {
                    ID = SqlHelper.GetInt(reader, "UserId"),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                };

                if (!reader.IsDBNull(reader.GetOrdinal("ImageId")))
                    href.ProfilePhotoId = SqlHelper.GetInt(reader, "ImageId");

                list.Add(href);
            }
        }

        return list;
    }

    public async Task<int> AddGuest(int hardheadID, int guestID, int userID, DateTime actionDate)
    {
        var sql = @"INSERT INTO [dbo].[rep_User] ([EventId],[TypeId],[UserId],[Inserted],[InsertedBy])
                        VALUES (@hardheadID, 52, @guestID, @insertedDate, @userID)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("hardheadID", hardheadID));
        command.Parameters.Add(new SqlParameter("guestID", guestID));
        command.Parameters.Add(new SqlParameter("insertedDate", actionDate));
        command.Parameters.Add(new SqlParameter("userID", userID));

        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> RemoveGuest(int hardheadID, int guestID)
    {
        var sql = @"DELETE FROM [dbo].[rep_User]
                        WHERE TypeId = 52 AND eventID = @hardheadID AND userID = @guestID";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("hardheadID", hardheadID));
        command.Parameters.Add(new SqlParameter("guestID", guestID));

        return await command.ExecuteNonQueryAsync();
    }
}
