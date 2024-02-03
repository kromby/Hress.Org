using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Ez.Hress.UserProfile.Entities;
using Ez.Hress.UserProfile.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.UserProfile.DataAccess
{
    public class UserProfileSqlDataAccess : IUserProfileDataAccess
    {
        private readonly ILogger<UserProfileSqlDataAccess> _log;
        private readonly string _connectionString;
        private readonly string _class;

        public UserProfileSqlDataAccess(DbConnectionInfo connectionInfo, ILogger<UserProfileSqlDataAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
            _class = nameof(UserProfileSqlDataAccess);
        }

        public async Task<UserBasicEntity?> GetUser(int userID)
        {
            const string sql = @"SELECT	usr.Id, usr.Username, usr.Inserted, img.ImageID, tName.TextValue 'Name'
                                FROM	adm_User usr
                                LEFT OUTER JOIN	upf_Image img ON img.UserID = usr.ID AND img.TypeId = 14
                                LEFT OUTER JOIN upf_Text tName ON tName.UserId = usr.ID AND tName.TypeId = 83
                                WHERE	usr.Id = @userID
                                AND	usr.Deleted IS NULL";

            _log.LogInformation("[{Class}.{Method}] userID: {userID}", _class, nameof(GetUser), userID);
            _log.LogInformation("[{Class}.{Method}] Executing SQL: '{SQL}'", _class, nameof(GetUser), sql);

            UserBasicEntity user = new();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("userID", userID);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                user.ID = SqlHelper.GetInt(reader, "ID");
                user.Username = reader.GetString(reader.GetOrdinal("Username"));
                user.ProfilePhotoId = SqlHelper.GetInt(reader, "ImageID");
                user.Inserted = SqlHelper.GetDateTime(reader, "Inserted");
                user.Name = reader.GetString(reader.GetOrdinal("Name")); ;
                return user;
            }
            return null;
        }

        public async Task<IList<Relation>> GetRelations(int userID)
        {
            const string sql = @"SELECT	rel.Id, rel.PrimaryUserId, prim.Username 'PrimaryUsername', primUserPhoto.ImageId 'PrimaryImageId', 
		                                rel.SecondaryUserId, sec.Username 'SecondaryUsername', secUserPhoto.ImageId 'SecondaryImageId', 
		                                rel.TypeId, rel.Inserted, rel.InsertedBy, typ.Name, typ.Shortcode, typ.Description
                                FROM	upf_Relation rel
                                JOIN	gen_Type typ ON typ.Id = rel.TypeId AND typ.Id != 114
                                JOIN	adm_User prim ON rel.PrimaryUserId = prim.Id
                                LEFT OUTER JOIN	upf_Image primUserPhoto ON rel.PrimaryUserId = primUserPhoto.UserId AND primUserPhoto.TypeId = 14
                                JOIN	adm_User sec ON rel.SecondaryUserId = sec.Id
                                LEFT OUTER JOIN	upf_Image secUserPhoto ON rel.SecondaryUserId = secUserPhoto.UserId AND secUserPhoto.TypeId = 14
                                WHERE	(rel.PrimaryUserId = @userID
	                                OR	rel.SecondaryUserId = @userID)
	                                AND	rel.Deleted IS NULL";
            _log.LogInformation("[{Class}.{Method}] userID: {userID}", _class, nameof(GetRelations), userID);
            _log.LogInformation("[{Class}.{Method}] Executing SQL: '{SQL}'", _class, nameof(GetRelations), sql);

            var list = new List<Relation>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("userID", userID);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int primUserID = SqlHelper.GetInt(reader, "PrimaryUserId");
                    string prefix = primUserID != userID ? "Primary" : "Secondary";

                    var relatedUser = new UserBasicEntity()
                    {
                        ID = SqlHelper.GetInt(reader, $"{prefix}UserId"),
                        Username = reader.GetString(reader.GetOrdinal($"{prefix}Username")),
                        ProfilePhotoId = SqlHelper.GetInt(reader, $"{prefix}ImageId")
                    };

                    var type = new TypeEntity(SqlHelper.GetInt(reader, "TypeId"), reader.GetString(reader.GetOrdinal("Name")), reader.GetString(reader.GetOrdinal("Shortcode")))
                    {
                        Description = reader.GetString(reader.GetOrdinal("Description"))
                    };

                    var entity = new Relation(SqlHelper.GetInt(reader, "ID"), relatedUser, type)
                    {
                        Name = type.Name,
                        Inserted = SqlHelper.GetDateTime(reader, "Inserted"),
                        InsertedBy = SqlHelper.GetInt(reader, "InsertedBy")
                    };

                    list.Add(entity);
                }
            }

            return list;
        }

        public async Task<IList<Transaction>> GetTransactions(int userID)
        {
            const string sql = @"SELECT	debt.ID, debt.UserID, usr.Username, userPhoto.ImageId 'UserImageID', debt.Name, debt.Amount, debt.Inserted, debt.InsertedBy
                                FROM	upf_Debt debt
								JOIN	adm_User usr ON debt.UserID = usr.Id
								LEFT OUTER JOIN	upf_Image userPhoto ON debt.UserID = userPhoto.UserId AND userPhoto.TypeId = 14
                                WHERE	debt.UserID = @userID AND debt.Deleted IS NULL
                                ORDER BY debt.Inserted";

            _log.LogInformation("[{Class}.{Method}] userID: {userID}", _class, nameof(GetTransactions), userID);
            _log.LogInformation("[{Class}.{Method}] Executing SQL: '{SQL}'", _class, nameof(GetTransactions), sql);

            var list = new List<Transaction>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("userID", userID);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var user = new UserBasicEntity()
                    {
                        ID = SqlHelper.GetInt(reader, "UserID"),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        ProfilePhotoId = SqlHelper.GetInt(reader, "UserImageID")
                    };

                    var entity = new Transaction(SqlHelper.GetInt(reader, "ID"), SqlHelper.GetInt(reader, "Amount"), reader.GetString(reader.GetOrdinal("Name")), user)
                    {
                        Inserted = SqlHelper.GetDateTime(reader, "Inserted"),
                        InsertedBy = SqlHelper.GetInt(reader, "InsertedBy")
                    };

                    list.Add(entity);
                }
            }

            return list;
        }
    }
}
