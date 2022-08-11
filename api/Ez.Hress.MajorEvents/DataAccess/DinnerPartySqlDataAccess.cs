using Ez.Hress.MajorEvents.Entities;
using Ez.Hress.MajorEvents.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.MajorEvents.DataAccess
{
    public class DinnerPartySqlDataAccess : IDinnerPartyDataAccess
    {
        private readonly string _connectionString;
        private readonly ILogger<DinnerPartySqlDataAccess> _log;
        public DinnerPartySqlDataAccess(DbConnectionInfo connectionInfo, ILogger<DinnerPartySqlDataAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }

        public async Task<IList<DinnerParty>> GetAll()
        {
            var sql = @"SELECT	ROW_NUMBER() OVER(ORDER BY dinner.Inserted ASC) 'Number', dinner.Id, dinner.Date, 
		                        dinner.Inserted, dinner.InsertedBy, tLocation.TextValue 'Location', theme.TextValue 'Theme', img.ImageId, COUNT(guest.Id) 'GuestCount'
                        FROM	rep_Event dinner
                        JOIN	rep_Text tLocation ON tLocation.EventId = dinner.Id AND tLocation.TypeId = 67
						JOIN	rep_User guest ON guest.EventId = dinner.Id
                        LEFT OUTER JOIN	rep_Text theme ON theme.EventId = dinner.Id AND theme.TypeId = 194
                        LEFT OUTER JOIN rep_Image img ON img.EventId = dinner.Id AND img.TypeId = 14
                        WHERE	dinner.TypeId = 56 AND dinner.ParentId IS NULL
						GROUP BY dinner.Id, dinner.Date, dinner.Inserted, dinner.InsertedBy, tLocation.TextValue, theme.TextValue, img.ImageId
                        ORDER BY dinner.Inserted DESC";

            _log.LogInformation("[{Class}] GetAll", this.GetType().Name);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;

            var list = new List<DinnerParty>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    DinnerParty entity = new(SqlHelper.GetInt(reader, "ID"),
                        (int)reader.GetInt64(reader.GetOrdinal("Number")),
                        SqlHelper.GetDateTime(reader, "Date"),
                        reader.GetString(reader.GetOrdinal("Location")))
                    {
                        Inserted = SqlHelper.GetDateTime(reader, "Inserted"),
                        InsertedBy = SqlHelper.GetInt(reader, "InsertedBy"),
                        GuestCount = SqlHelper.GetInt(reader, "GuestCount")
                    };

                    if (!reader.IsDBNull(reader.GetOrdinal("Theme")))
                    {
                        entity.Theme = reader.GetString(reader.GetOrdinal("Theme"));
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("ImageId")))
                    {
                        entity.CoverImage = new ImageHrefEntity(SqlHelper.GetInt(reader, "ImageId"), entity.Name);
                    }

                    list.Add(entity);
                }
            }

            return list;
        }

        public Task<DinnerParty> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Course>> GetCoursesByTypeId(int typeID)
        {
            var sql = @"SELECT	txt.Id, txt.EventId, txt.TextValue, mor.Number 'Year'
                        FROM	rep_Text txt
                        JOIN	rep_Event mor ON txt.EventID = mor.Id
                        WHERE	txt.TypeId = @typeID
	                        AND	txt.Id NOT IN (22900, 23053, 23195, 23270, 23434, 23446, 23515, 23576, 23669)
                        ORDER BY txt.TextValue";

            _log.LogInformation("[{Class}] GetCoursesByTypeId", this.GetType().Name);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;
            command.Parameters.AddWithValue("typeID", typeID);

            var list = new List<Course>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    Course entity = new(SqlHelper.GetInt(reader, "Id"), SqlHelper.GetInt(reader,"EventId"), reader.GetString(reader.GetOrdinal("TextValue")), SqlHelper.GetInt(reader, "Year"));
                    list.Add(entity);
                }
            }

            return list;
        }

        public async Task<IList<Guest>> GetGuests(int partyID, int? typeID)
        {
            string typeWhere = string.Empty;
            if(typeID.HasValue)
            {
                typeWhere = " AND typ.ID = @typeID ";
            }

            var sql = $@"SELECT	guest.UserId, usr.Username, userPhoto.ImageId 'ImageID', typ.Name
                        FROM rep_User guest
                        JOIN adm_User usr ON guest.UserId = usr.Id
                        JOIN gen_Type typ ON guest.TypeId = typ.Id
                        LEFT OUTER JOIN upf_Image userPhoto ON usr.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                        WHERE guest.EventId = @partyID
                        {typeWhere}
                        ORDER BY typ.Id DESC, usr.Username";

            _log.LogInformation("[{Class}] GetGuests", this.GetType().Name);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;

            command.Parameters.AddWithValue("partyID", partyID);
            if(typeID.HasValue)
            {
                command.Parameters.AddWithValue("typeID", typeID.Value);
            }

            var list = new List<Guest>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    Guest guest = new(SqlHelper.GetInt(reader, "UserId"), reader.GetString(reader.GetOrdinal("Username")), SqlHelper.GetInt(reader, "ImageID"), reader.GetString(reader.GetOrdinal("Name")));
                    list.Add(guest);
                }
            }

            return list;
        }
    }
}
