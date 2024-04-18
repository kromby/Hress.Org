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

        public async Task<IList<News>> GetLatestNews(int top)
        {
            var sql = @"SELECT	TOP (@top) news.Id, news.Name, news.Hits, news.Inserted, news.InsertedBy, author.Username, userPhoto.ImageId 'AuthorImageID', news.Updated, news.UpdatedBy, body.TextValue 'Body', img.Align, img.ImageId, gImg.Description 'ImageName', gImg.Height, gImg.Width
                        FROM	adm_Component news
                        JOIN	adm_User author ON author.Id = news.InsertedBy
                        JOIN	scr_Text body ON body.ComponentId = news.Id AND body.TypeId = 36
                        LEFT OUTER JOIN	upf_Image userPhoto ON author.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                        LEFT OUTER JOIN	scr_Image img ON img.ComponentId = news.Id
						LEFT OUTER JOIN gen_Image gImg ON img.ImageId = gImg.Id
                        WHERE	news.TypeId = 9
	                        AND	news.Deleted IS NULL
                        ORDER BY news.Inserted DESC";

            _log.LogInformation("[{Class}] top: {top}", nameof(GetLatestNews), top);
            _log.LogInformation("[{Class}] Executing SQL: '{SQL}'", nameof(GetLatestNews), sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;
            command.Parameters.AddWithValue("top", top);

            var list = new List<News>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    News entity = ParseNews(reader);

                    list.Add(entity);
                }
            }

            return list;
        }

        private static News ParseNews(SqlDataReader reader)
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
                entity.Image = new ImageHrefEntity(SqlHelper.GetInt(reader, "ImageID"), reader.GetString(reader.GetOrdinal("ImageName")))
                {
                    Height = SqlHelper.GetInt(reader, "Height"),
                    Width = SqlHelper.GetInt(reader, "Width")
                };

                entity.ImageAlign = Enum.Parse<Align>(reader.GetInt32(reader.GetOrdinal("Align")).ToString());
            }
            if (!reader.IsDBNull(reader.GetOrdinal("AuthorImageID")))
            {
                entity.Author.ProfilePhotoId = reader.GetInt32(reader.GetOrdinal("AuthorImageID"));
            }

            return entity;
        }

        public async Task<News> GetNews(int id)
        {
            var sql = @"SELECT	news.Id, news.Name, news.Hits, news.Inserted, news.InsertedBy, author.Username, userPhoto.ImageId 'AuthorImageID', news.Updated, news.UpdatedBy, body.TextValue 'Body', img.Align, img.ImageId, gImg.Description 'ImageName', gImg.Height, gImg.Width
                        FROM	adm_Component news
                        JOIN	adm_User author ON author.Id = news.InsertedBy
                        JOIN	scr_Text body ON body.ComponentId = news.Id AND body.TypeId = 36
                        LEFT OUTER JOIN	upf_Image userPhoto ON author.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                        LEFT OUTER JOIN	scr_Image img ON img.ComponentId = news.Id
						LEFT OUTER JOIN gen_Image gImg ON img.ImageId = gImg.Id
                        WHERE	news.Id = @id
                            AND	news.TypeId = 9
	                        AND	news.Deleted IS NULL
                        ORDER BY news.Inserted DESC";

            _log.LogInformation("[{Class}] id: {id}", nameof(GetLatestNews), id);
            _log.LogInformation("[{Class}] Executing SQL: '{SQL}'", nameof(GetLatestNews), sql);

            News entity = new();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using var command = new SqlCommand(sql);
                command.Connection = connection;
                command.Parameters.AddWithValue("id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    reader.Read();
                    entity = ParseNews(reader);
                }
            }
            return entity;
        }

        public async Task<IList<News>> GetNews(DateTime date, bool useDay, bool useMonth, bool useYear)
        {
            StringBuilder where = new();
            if (useDay)
            {
                where.Append("AND DATEPART(DAY, news.Inserted) = @day ");
            }
            if (useMonth)
            {
                where.Append("AND DATEPART(MONTH,	news.Inserted) = @month ");
            }
            if (useYear)
            {
                where.Append("AND DATEPART(YEAR,	news.Inserted) = @year ");
            }

            var sql = $@"SELECT	news.Id, news.Name, news.Hits, news.Inserted, news.InsertedBy, author.Username, userPhoto.ImageId 'AuthorImageID', news.Updated, news.UpdatedBy, body.TextValue 'Body', img.Align, img.ImageId, gImg.Description 'ImageName', gImg.Height, gImg.Width
                        FROM	adm_Component news
                        JOIN	adm_User author ON author.Id = news.InsertedBy
                        JOIN	scr_Text body ON body.ComponentId = news.Id AND body.TypeId = 36
                        LEFT OUTER JOIN	upf_Image userPhoto ON author.Id = userPhoto.UserId AND userPhoto.TypeId = 14
                        LEFT OUTER JOIN	scr_Image img ON img.ComponentId = news.Id
						LEFT OUTER JOIN gen_Image gImg ON img.ImageId = gImg.Id
                        WHERE	news.TypeId = 9
	                        AND	news.Deleted IS NULL
							{where}
                        ORDER BY news.Inserted DESC";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;

            if (useDay)
            {
                command.Parameters.AddWithValue("day", date.Day);
            }
            if (useMonth)
            {
                command.Parameters.AddWithValue("month", date.Month);
            }
            if (useYear)
            {
                command.Parameters.AddWithValue("year", date.Year);
            }

            var list = new List<News>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    News entity = ParseNews(reader);

                    list.Add(entity);
                }
            }

            return list;
        }

        public async Task<IList<StatisticNewsByDate>> GetNewsStatisticsByDate(int? year)
        {
            string sql = year.HasValue
                ? @"SELECT	DATEPART(month, news.Inserted) 'month', COUNT(news.Id) 'counter'
                        FROM	adm_Component news
                        WHERE	news.TypeId = 9
	                        AND	news.Deleted IS NULL
							AND	DATEPART(year, news.Inserted) = @year
						GROUP BY DATEPART(month, news.Inserted)
						ORDER BY DATEPART(month, news.Inserted) DESC"
                : @"SELECT	DATEPART(year, news.Inserted) 'year', COUNT(news.Id) 'counter'
                        FROM	adm_Component news
                        WHERE	news.TypeId = 9
	                        AND	news.Deleted IS NULL
						GROUP BY DATEPART(year, news.Inserted)
						ORDER BY DATEPART(year, news.Inserted) DESC";
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql);
            command.Connection = connection;

            var list = new List<StatisticNewsByDate>();

            if (year.HasValue)
            {
                command.Parameters.AddWithValue("year", year.Value);
            }

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    StatisticNewsByDate entity = year.HasValue
                        ? new(DatePart.Month,
                        SqlHelper.GetInt(reader, "month"),
                        SqlHelper.GetInt(reader, "counter"))
                        : new(DatePart.Year,
                        SqlHelper.GetInt(reader, "year"),
                        SqlHelper.GetInt(reader, "counter"));
                    list.Add(entity);
                }
            }

            return list;
        }
    }
}
