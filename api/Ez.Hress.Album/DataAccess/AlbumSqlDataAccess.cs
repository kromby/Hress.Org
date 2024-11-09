using Ez.Hress.Albums.Entities;
using Ez.Hress.Albums.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Ez.Hress.Albums.DataAccess;

public class AlbumSqlDataAccess : IAlbumDataAccess
{
    private readonly ILogger<AlbumSqlDataAccess> _log;
    private readonly string _connectionString;

    public AlbumSqlDataAccess(DbConnectionInfo dbConnection, ILogger<AlbumSqlDataAccess> log)
    {
        _connectionString = dbConnection.ConnectionString;
        _log = log;
    }

    public async Task<Album?> GetAlbum(int id)
    {
        const string sql = @"SELECT	album.Id, album.Name, album.Description, album.Inserted, album.InsertedBy, COUNT(img.Id) 'ImageCount'
                                    FROM	adm_Component album
                                    LEFT OUTER JOIN	scr_Image img ON img.ComponentId = album.Id
                                    WHERE	album.TypeId = 43
	                                    AND	album.Deleted IS NULL
                                        AND album.Id = @id
                                    GROUP BY album.Id, album.Name, album.Description, album.Inserted, album.InsertedBy
                                    ORDER BY album.Inserted DESC ";

        _log.LogInformation("[{Method}] Executing SQL: '{SQL}'", nameof(GetAlbum), sql);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql)
        {
            Connection = connection
        };
        command.Parameters.AddWithValue("id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (reader.Read())
        {
            return ParseAlbum(reader);
        }

        return null;
    }

    public async Task<IList<Album>> GetAlbums()
    {
        const string sql = @"SELECT	album.Id, album.Name, album.Description, album.Inserted, album.InsertedBy, COUNT(img.Id) 'ImageCount'
                                    FROM	adm_Component album
                                    JOIN	scr_Image img ON img.ComponentId = album.Id
                                    WHERE	album.TypeId = 43
	                                    AND	album.Deleted IS NULL
                                    GROUP BY album.Id, album.Name, album.Description, album.Inserted, album.InsertedBy
                                    ORDER BY album.Inserted DESC ";

        _log.LogInformation("[{Method}] Executing SQL: '{SQL}'", nameof(GetAlbums), sql);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql)
        {
            Connection = connection
        };

        var list = new List<Album>();

        using (var reader = await command.ExecuteReaderAsync())
        {
            while (reader.Read())
            {
                Album entity = ParseAlbum(reader);
                list.Add(entity);
            }
        }

        return list;
    }

    private static Album ParseAlbum(SqlDataReader reader)
    {
        Album entity = new(
            SqlHelper.GetInt(reader, "Id"),
            reader.GetString(reader.GetOrdinal("Name")),
            reader.GetString(reader.GetOrdinal("Description")),
            SqlHelper.GetInt(reader, "ImageCount"))
        {
            Inserted = SqlHelper.GetDateTime(reader, "Inserted"),
            InsertedBy = SqlHelper.GetInt(reader, "InsertedBy")
        };
        return entity;
    }

    public async Task<IList<ImageEntity>> GetImages(int albumID)
    {
        const string sql = @"SELECT	gImg.Id, gImg.Description, gImg.Source, gImg.Inserted
                                FROM	scr_Image img
                                JOIN	gen_Image gImg ON img.ImageId = gImg.Id
                                WHERE	img.ComponentId = @id";

        _log.LogInformation("[{Method}] AlbumID: '{albumID}'", nameof(GetImages), albumID);
        _log.LogInformation("[{Method}] Executing SQL: '{SQL}'", nameof(GetImages), sql);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql)
        {
            Connection = connection
        };
        command.Parameters.AddWithValue("id", albumID);

        var list = new List<ImageEntity>();

        using (var reader = await command.ExecuteReaderAsync())
        {
            while (reader.Read())
            {
                var entity = new ImageEntity(
                        SqlHelper.GetInt(reader, "Id"),
                        reader.GetString(reader.GetOrdinal("Description")),
                        reader.GetString(reader.GetOrdinal("Source")))
                {
                    Inserted = SqlHelper.GetDateTime(reader, "Inserted")
                };
                list.Add(entity);
            }
        }

        return list;
    }

    public async Task<Album> CreateAlbum(Album album)
    {
        const string sql = @"INSERT INTO adm_Component (TypeId, GroupType, IsPublic, Name, Description, InsertedBy, Inserted)
                            OUTPUT INSERTED.Id
                            VALUES (43, 'CONTENT', 0, @Name, @Description, @InsertedBy, @Inserted)";

        _log.LogInformation("[{Method}] Executing SQL: '{SQL}'", nameof(CreateAlbum), sql);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql)
        {
            Connection = connection
        };

        command.Parameters.AddWithValue("@Name", album.Name);
        command.Parameters.AddWithValue("@Description", album.Description);
        command.Parameters.AddWithValue("@InsertedBy", album.InsertedBy);
        command.Parameters.AddWithValue("@Inserted", album.Inserted);

        var result = await command.ExecuteScalarAsync();
        if (result is null)
            throw new InvalidOperationException("Failed to get inserted ID");

        album.ID = (int)result;
        return album;
    }
}
