using Azure.Data.Tables;
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
        private readonly TableClient _tableClient;
        private readonly ILogger<DinnerPartySqlDataAccess> _log;
        public DinnerPartySqlDataAccess(DbConnectionInfo connectionInfo, IList<TableClient> clients, ILogger<DinnerPartySqlDataAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _tableClient = clients.First(t => t.Name == "DinnerPartyElection");
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

        public async Task<DinnerParty?> GetById(int id)
        {
            var sql = @"SELECT	dinner.Id, dinner.Date, 
		                        dinner.Inserted, dinner.InsertedBy, tLocation.TextValue 'Location', theme.TextValue 'Theme', img.ImageId, COUNT(guest.Id) 'GuestCount'
                        FROM	rep_Event dinner
                        JOIN	rep_Text tLocation ON tLocation.EventId = dinner.Id AND tLocation.TypeId = 67
						JOIN	rep_User guest ON guest.EventId = dinner.Id
                        LEFT OUTER JOIN	rep_Text theme ON theme.EventId = dinner.Id AND theme.TypeId = 194
                        LEFT OUTER JOIN rep_Image img ON img.EventId = dinner.Id AND img.TypeId = 14
                        WHERE	dinner.TypeId = 56 AND dinner.ParentId IS NULL AND dinner.Id = @id
						GROUP BY dinner.Id, dinner.Date, dinner.Inserted, dinner.InsertedBy, tLocation.TextValue, theme.TextValue, img.ImageId
                        ORDER BY dinner.Inserted DESC";

            _log.LogInformation("[{Class}] GetById ID: {id}", this.GetType().Name, id);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;

            command.Parameters.AddWithValue("@id", id);

            DinnerParty? entity = null;

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    entity = new(SqlHelper.GetInt(reader, "ID"),
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
                }
            }

            return entity;
        }

        public async Task<IList<PartyTeam>> GetChilds(int partyID)
        {
            const string sql = @"SELECT	team.Id, team.Number, wine.TextValue 'Wine', winner.TextValue 'IsWinner', CAST(CASE WHEN LEN(quiz.TextValue) > 0 THEN 1 ELSE 0 END AS bit) hasQuiz
                                FROM	rep_Event team
                                LEFT OUTER JOIN	rep_Text wine ON wine.EventId = team.Id AND wine.TypeId = 128
                                LEFT OUTER JOIN	rep_Text winner ON winner.EventId = team.Id AND winner.TypeId = 200
								LEFT OUTER JOIN	(SELECT q.EventId, MAX(q.TextValue) TextValue FROM rep_Text q WHERE q.TypeId = 122 GROUP BY q.EventId) quiz ON quiz.EventId = team.Id
                                WHERE	team.ParentId = @partyID
                                ORDER BY team.Number";

            _log.LogInformation("[{Class}] GetChilds ID: {partyID}", this.GetType().Name, partyID);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;

            command.Parameters.AddWithValue("@partyID", partyID);

            var list = new List<PartyTeam>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var isWinner = false;
                    if (!reader.IsDBNull(reader.GetOrdinal("IsWinner")))
                    {
                        var winnerString = reader.GetString(reader.GetOrdinal("isWinner"));
                        var parsed = bool.TryParse(winnerString, out isWinner);
                        isWinner = parsed && isWinner;
                    }
                    var entity = new PartyTeam(SqlHelper.GetInt(reader, "ID"), SqlHelper.GetInt(reader, "Number"), isWinner);
                    if (!reader.IsDBNull(reader.GetOrdinal("Wine")))
                    {
                        entity.Wine = reader.GetString(reader.GetOrdinal("Wine"));
                    }

                    entity.HasQuiz = SqlHelper.GetBool(reader, "HasQuiz");

                    list.Add(entity);
                }
            }

            return list;
        }

        public async Task<IList<PartyUser>> GetChildUsers(int partyID)
        {
            const string sql = @"SELECT	member.EventId, member.UserId, usr.Username, uImg.ImageId
                                FROM	rep_User member
                                JOIN	adm_User usr ON member.UserId = usr.Id
                                LEFT OUTER JOIN	upf_Image uImg ON uImg.UserId = usr.Id AND uImg.TypeId = 14
                                WHERE	member.GroupId = @partyID";

            _log.LogInformation("[{Class}] GetChildUsers ID: {partyID}", this.GetType().Name, partyID);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;

            command.Parameters.AddWithValue("@partyID", partyID);

            var list = new List<PartyUser>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var user = new PartyUser(SqlHelper.GetInt(reader, "UserID"), SqlHelper.GetInt(reader, "EventID"), reader.GetString(reader.GetOrdinal("Username")), SqlHelper.GetInt(reader, "ImageId"), "Meðlimur");

                    list.Add(user);
                }
            }

            return list;
        }

        public async Task<IList<Course>> GetCourses(int partyID)
        {
            var sql = @"SELECT	txt.Id, txt.EventId, txt.TextValue, txt.TypeId, typ.Name 'TypeName'
                        FROM	rep_Text txt
						JOIN	gen_Type typ ON txt.TypeId = typ.Id
                        WHERE	txt.EventId = @partyID
							AND	txt.TypeId IN (191, 192, 193, 223, 224)
                        ORDER BY typ.Description";

            _log.LogInformation("[{Class}] GetCoursesByTypeId", this.GetType().Name);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;
            command.Parameters.AddWithValue("partyID", partyID);

            var list = new List<Course>();
            Course current = new(-1, "");

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    int typeID = SqlHelper.GetInt(reader, "TypeID");
                    if (current.ID != typeID)
                    {
                        if (current.ID != -1)
                        {
                            list.Add(current);
                        }
                        current = new Course(typeID, reader.GetString(reader.GetOrdinal("TypeName")));
                    }

                    current.Dishes.Add(new(SqlHelper.GetInt(reader, "Id"), SqlHelper.GetInt(reader, "EventId"), reader.GetString(reader.GetOrdinal("TextValue"))));
                }

                list.Add(current);
            }

            return list;
        }

        public async Task<IList<Dish>> GetCoursesByTypeId(int typeID)
        {
            var sql = @"SELECT	txt.Id, txt.EventId, txt.TextValue, mor.Number 'Year'
                        FROM	rep_Text txt
                        JOIN	rep_Event mor ON txt.EventID = mor.Id
                        WHERE	txt.TypeId = @typeID
                        ORDER BY txt.TextValue";

            _log.LogInformation("[{Class}] GetCoursesByTypeId", this.GetType().Name);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;
            command.Parameters.AddWithValue("typeID", typeID);

            var list = new List<Dish>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    Dish entity = new(SqlHelper.GetInt(reader, "Id"), SqlHelper.GetInt(reader, "EventId"), reader.GetString(reader.GetOrdinal("TextValue")), SqlHelper.GetInt(reader, "Year"));
                    list.Add(entity);
                }
            }

            return list;
        }

        public async Task<IList<PartyUser>> GetGuests(int partyID, int? typeID)        
        {
            string typeWhere = string.Empty;
            if (typeID.HasValue)
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
            if (typeID.HasValue)
            {
                command.Parameters.AddWithValue("typeID", typeID.Value);
            }

            var list = new List<PartyUser>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    PartyUser guest = new(SqlHelper.GetInt(reader, "UserId"), partyID, reader.GetString(reader.GetOrdinal("Username")), SqlHelper.GetInt(reader, "ImageID"), reader.GetString(reader.GetOrdinal("Name")));
                    list.Add(guest);
                }
            }

            return list;
        }

        public async Task<IList<NameHrefEntity<int>>> GetAlbums(int partyID)
        {
            var sql = @"SELECT	tx.TextValue, c.Name, c.Description
                        FROM	rep_Text tx
                        JOIN	adm_Component c ON tx.TextValue = c.Id
                        WHERE	tx.EventId = @id
	                        AND	tx.TypeId = 190";

            _log.LogInformation("[{Class}] GetAlbums SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;

            command.Parameters.AddWithValue("id", partyID);

            var list = new List<NameHrefEntity<int>>();
            using(var reader = await command.ExecuteReaderAsync())
            {
                while(reader.Read())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("TextValue")))
                    {
                        NameHrefEntity<int> entity = new(int.Parse(reader.GetString(reader.GetOrdinal("TextValue"))))
                        {
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.GetString(reader.GetOrdinal("Description"))
                        };
                        list.Add(entity);
                    }                    
                }
            }

            return list;
        }

        public async Task<IList<PartyQuiz>> GetQuizQuestions(int teamID)
        {
            var sql = @"SELECT	question.Id, question.TextValue 'question', answer.TextValue 'answer'
                        FROM	rep_Text question
                        JOIN	rep_Text answer ON question.Id = answer.ParentId
                        WHERE	question.EventId = @teamID
	                        AND question.TypeId = 122";

            _log.LogInformation("[{Class}] GetQuizQuestions", this.GetType().Name);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;
            command.Parameters.AddWithValue("teamID", teamID);
            var list = new List<PartyQuiz>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    PartyQuiz quiz = new(SqlHelper.GetInt(reader, "Id"), reader.GetString(reader.GetOrdinal("Question")), reader.GetString(reader.GetOrdinal("Answer")));
                    list.Add(quiz);
                }
            }

            return list;
        }

        public async Task<IList<PartyWinner>> GetGuestStatistic()
        {
            var sql = @"SELECT	guest.UserId, COUNT(guest.EventId) GuestCount, usr.Username, userPhoto.ImageId 'ImageID'--, guest.TypeId
                        FROM rep_User guest
                        JOIN rep_Event mogr ON mogr.Id = guest.EventId AND mogr.TypeId = 56
                        JOIN adm_User usr ON guest.UserId = usr.Id
                        LEFT OUTER JOIN upf_Image userPhoto ON usr.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                        WHERE guest.TypeId NOT IN (54, 81)
                        GROUP BY guest.UserId, usr.Username, userPhoto.ImageId--, guest.TypeId
                        ORDER BY COUNT(guest.EventId) DESC";

            _log.LogInformation("[{Class}] GetWinnerStatistic", this.GetType().Name);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;

            var list = new List<PartyWinner>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    PartyWinner guest = new(SqlHelper.GetInt(reader, "UserId"), reader.GetString(reader.GetOrdinal("Username")), SqlHelper.GetInt(reader, "GuestCount"))
                    {
                        ProfilePhotoId = SqlHelper.GetInt(reader, "ImageID")
                    };
                    list.Add(guest);
                }
            }

            return list;
        }

        public async Task<IList<PartyWinner>> GetWinnerStatistic()
        {
            var sql = @"SELECT	usr.Id, usr.Username, COUNT(usr.Id) WinCount
                        FROM	rep_Text textWinner
                        JOIN	rep_Event team ON textWinner.EventId = team.Id
                        JOIN	rep_User member ON team.Id = member.EventId
                        JOIN	adm_User usr ON member.UserId = usr.Id
                        WHERE	textWinner.TypeId = 200
                        GROUP BY usr.Id, usr.Username
                        ORDER BY COUNT(usr.Id) DESC";

            _log.LogInformation("[{Class}] GetWinnerStatistic", this.GetType().Name);
            _log.LogInformation("[{Class}] Executing SQL: {sql}", this.GetType().Name, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;

            var list = new List<PartyWinner>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    PartyWinner winner = new(SqlHelper.GetInt(reader, "Id"), reader.GetString(reader.GetOrdinal("Username")), SqlHelper.GetInt(reader, "WinCount"));
                    list.Add(winner);
                }
            }

            return list;
        }

        public async Task SaveVote(Vote vote)
        {
            _log.LogInformation("[{Class}] SaveVote", this.GetType().Name);

            VoteTableEntity entity = new(vote);
            await _tableClient.UpsertEntityAsync<VoteTableEntity>(entity);
            return;
        }
    }
}
