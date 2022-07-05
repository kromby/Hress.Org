using Ez.Hress.Scripts.Entities;
using Ez.Hress.Scripts.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Scripts.DataAccess
{
    public class NewsSqlDataAccess : INewsDataAccess
    {
        private readonly ILogger<NewsSqlDataAccess> _log;
        private readonly string _connectionString;

        public NewsSqlDataAccess(DbConnectionInfo dbConnection, ILogger<NewsSqlDataAccess> log)
        {
            _connectionString = dbConnection.ConnectionString;
            _log = log;
        }

        public async Task<IList<News>> GetNews(int top)
        {
            var sql = @"SELECT	TOP (@top) news.Id, news.Name, news.Hits, news.Inserted, news.InsertedBy, author.Username, userPhoto.ImageId 'AuthorImageID', news.Updated, news.UpdatedBy, body.TextValue 'Body', img.Align, img.ImageId
                        FROM	adm_Component news
                        JOIN	adm_User author ON author.Id = news.InsertedBy
                        JOIN	scr_Text body ON body.ComponentId = news.Id AND body.TypeId = 36
                        LEFT OUTER JOIN	upf_Image userPhoto ON author.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                        LEFT OUTER JOIN	scr_Image img ON img.ComponentId = news.Id
                        WHERE	news.TypeId = 9
	                        AND	news.Deleted IS NULL
                        ORDER BY news.Inserted DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(sql))
                {
                    command.Connection = connection;
                    command.Parameters.AddWithValue("top", top);

                    var list = new List<News>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var entity = new News()
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Inserted = reader.GetDateTime(reader.GetOrdinal("Inserted")),
                                InsertedBy = reader.GetInt32(reader.GetOrdinal("InsertedBy")),
                                Updated = SqlHelper.GetDateTimeNullable(reader, "Updated"),
                                UpdatedBy = SqlHelper.GetNullableInt(reader, "UpdatedBy"),
                                Content = reader.GetString(reader.GetOrdinal("Body")),
                                Author = new UserBasicEntity()
                                {
                                    ID = reader.GetInt32(reader.GetOrdinal("InsertedBy")),
                                    Username = reader.GetString(reader.GetOrdinal("Username")),
                                }
                            };                            
                            if (!reader.IsDBNull(reader.GetOrdinal("ImageID")))
                            {
                                entity.ImageID = reader.GetInt32(reader.GetOrdinal("ImageID"));
                                entity.ImageAlign = Enum.Parse<Align>(reader.GetInt32(reader.GetOrdinal("Align")).ToString());
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("AuthorImageID")))
                            {
                                entity.Author.ProfilePhotoId = reader.GetInt32(reader.GetOrdinal("AuthorImageID"));
                            }

                            list.Add(entity);
                        }
                    }

                    return list;
                }
            }
        }
    }
}
