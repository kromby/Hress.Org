using Ez.Hress.Scripts.Entities;
using Ez.Hress.Scripts.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Text;

namespace Ez.Hress.Scripts.DataAccess;

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

        using var command = new SqlCommand(sql)
        {
            Connection = connection
        };
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

    public async Task<IList<News>> GetNewsByAuthor(int userId)
    {
        const string sql = @"SELECT	news.Id, news.Name, news.Inserted, news.InsertedBy, news.Hits, news.IsPublic
                        FROM	adm_Component news
                        WHERE	news.TypeId = 9
                            AND	news.InsertedBy = @userId
                            AND	news.Deleted IS NULL
                        ORDER BY news.Inserted DESC";

        _log.LogInformation("[{Class}.{Method}] userId: {UserId}", nameof(NewsSqlDataAccess), nameof(GetNewsByAuthor), userId);
        _log.LogInformation("[{Class}.{Method}] Executing SQL: '{SQL}'", nameof(NewsSqlDataAccess), nameof(GetNewsByAuthor), sql);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql)
        {
            Connection = connection
        };
        command.Parameters.AddWithValue("userId", userId);

        var list = new List<News>();

        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                list.Add(new News()
                {
                    ID = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Inserted = reader.GetDateTime(reader.GetOrdinal("Inserted")),
                    InsertedBy = reader.GetInt32(reader.GetOrdinal("InsertedBy")),
                    Hits = GetHits(reader),
                    Public = SqlHelper.GetBool(reader, "IsPublic"),
                });
            }
        }

        _log.LogInformation("[{Class}.{Method}] Found {Count} news", nameof(NewsSqlDataAccess), nameof(GetNewsByAuthor), list.Count);
        return list;
    }

    public async Task<int> CreateNews(News news)
    {
        const string componentSql = @"INSERT INTO adm_Component (TypeId, GroupType, IsPublic, Name, Hits, InsertedBy, Inserted)
                            OUTPUT INSERTED.Id
                            VALUES (9, 'CONTENT', @IsPublic, @Name, 0, @InsertedBy, @Inserted)";
        const string textSql = @"INSERT INTO scr_Text (ComponentId, TypeId, TextValue, InsertedBy, Inserted)
                            VALUES (@ComponentId, @TypeId, @TextValue, @InsertedBy, @Inserted)";
        const string imageSql = @"INSERT INTO scr_Image (ComponentId, TypeId, ImageId, Align, InsertedBy, Inserted)
                            VALUES (@ComponentId, @TypeId, @ImageId, @Align, @InsertedBy, @Inserted)";

        _log.LogInformation("[{Class}.{Method}] Creating news '{Name}' for user '{UserId}'", nameof(NewsSqlDataAccess), nameof(CreateNews), news.Name, news.InsertedBy);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var subjectTypeId = await GetTypeId(connection, transaction, "TXT_SUBJECT");
            // Body text type is hardcoded as 36 in every read query, so it must match here.
            const int bodyTypeId = 36;

            var newsId = await InsertNewsComponent(connection, transaction, componentSql, news);

            foreach (var (typeId, text) in new[] { (subjectTypeId, news.Name!), (bodyTypeId, news.Content!) })
            {
                using var command = new SqlCommand(textSql, connection, transaction);
                command.Parameters.AddWithValue("@ComponentId", newsId);
                command.Parameters.AddWithValue("@TypeId", typeId);
                command.Parameters.AddWithValue("@TextValue", text);
                command.Parameters.AddWithValue("@InsertedBy", news.InsertedBy);
                command.Parameters.AddWithValue("@Inserted", news.Inserted);
                await command.ExecuteNonQueryAsync();
            }

            if (news.Image != null)
            {
                var mainImageTypeId = await GetTypeId(connection, transaction, "IMG_MAIN");
                using var command = new SqlCommand(imageSql, connection, transaction);
                command.Parameters.AddWithValue("@ComponentId", newsId);
                command.Parameters.AddWithValue("@TypeId", mainImageTypeId);
                command.Parameters.AddWithValue("@ImageId", news.Image.ID);
                command.Parameters.AddWithValue("@Align", (int)news.ImageAlign);
                command.Parameters.AddWithValue("@InsertedBy", news.InsertedBy);
                command.Parameters.AddWithValue("@Inserted", news.Inserted);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            _log.LogInformation("[{Class}.{Method}] Created news '{Id}'", nameof(NewsSqlDataAccess), nameof(CreateNews), newsId);
            return newsId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task<int> InsertNewsComponent(SqlConnection connection, SqlTransaction transaction, string sql, News news)
    {
        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@IsPublic", news.Public);
        command.Parameters.AddWithValue("@Name", news.Name);
        command.Parameters.AddWithValue("@InsertedBy", news.InsertedBy);
        command.Parameters.AddWithValue("@Inserted", news.Inserted);

        var result = await command.ExecuteScalarAsync();
        return result is int id ? id : throw new InvalidOperationException("Failed to get inserted news ID");
    }

    // Type ids are resolved by shortcode as the legacy NHibernate code did, rather than hardcoded.
    private static async Task<int> GetTypeId(SqlConnection connection, SqlTransaction transaction, string shortcode)
    {
        const string sql = "SELECT Id FROM gen_Type WHERE Shortcode = @Shortcode";

        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Shortcode", shortcode);

        var ids = new List<int>();
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                ids.Add(reader.GetInt32(0));
            }
        }

        if (ids.Count != 1)
        {
            throw new InvalidOperationException($"Expected exactly one gen_Type with shortcode '{shortcode}' but found {ids.Count}");
        }

        return ids[0];
    }

    /// <summary>
    /// Reads adm_Component.Hits null-safely. Uses Convert.ToInt32 so the mapping works
    /// regardless of the exact integer column type (int/bigint/smallint/decimal).
    /// </summary>
    private static int GetHits(SqlDataReader reader)
    {
        var ordinal = reader.GetOrdinal("Hits");
        return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
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
            Hits = GetHits(reader),
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

            using var command = new SqlCommand(sql)
            {
                Connection = connection
            };
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

        using var command = new SqlCommand(sql)
        {
            Connection = connection
        };

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

        using var command = new SqlCommand(sql)
        {
            Connection = connection
        };

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
