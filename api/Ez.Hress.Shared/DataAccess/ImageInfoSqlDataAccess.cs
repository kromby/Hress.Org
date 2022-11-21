using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.DataAccess
{
    public class ImageInfoSqlDataAccess : IImageInfoDataAccess
    {
        private readonly ILogger<ImageInfoSqlDataAccess> _log;
        private readonly string _connectionString;

        public ImageInfoSqlDataAccess(DbConnectionInfo connectionInfo, ILogger<ImageInfoSqlDataAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }

        public async Task<ImageEntity?> GetImage(int id)
        {
            var sql = @"SELECT	img.Id, img.Source, img.ThumbSource, img.Description, img.Inserted, img.InsertedBy
                        FROM gen_Image img
                        WHERE img.Deleted IS NULL
                            AND img.Id = @id";

            ImageEntity? entity = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("id", id);
                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    _log.LogInformation($"[{nameof(ImageInfoSqlDataAccess)}] Image '{id}' not found!");
                }

                entity = new(SqlHelper.GetInt(reader, "Id"), reader.GetString(reader.GetOrdinal("Description")), reader.GetString(reader.GetOrdinal("Source")))
                {
                    PhotoThumbUrl = reader.IsDBNull(reader.GetOrdinal("ThumbSource")) ? string.Empty : reader.GetString(reader.GetOrdinal("ThumbSource")),
                    Inserted = SqlHelper.GetDateTime(reader, "Inserted"),
                    InsertedBy = SqlHelper.GetInt(reader, "InsertedBy"),
                };
            }

            return entity;
        }

        public async Task<int> Save(ImageEntity entity, int typeID, int height, int width)
        {
            const string sql = @"INSERT INTO [dbo].[gen_Image]
                                   ([Source]
                                   ,[Description]
                                   ,[Height]
                                   ,[Width]
                                   ,[TypeId]
                                   ,[Inserted]
                                   ,[InsertedBy])
                                VALUES (@source, @description, @height, @width, @typeID, @inserted, @insertedBy);
                                SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("source", entity.PhotoUrl);
                command.Parameters.AddWithValue("description", entity.Name);
                command.Parameters.AddWithValue("height", height);
                command.Parameters.AddWithValue("width", width);
                command.Parameters.AddWithValue("typeID", typeID);
                command.Parameters.AddWithValue("inserted", entity.Inserted);
                command.Parameters.AddWithValue("insertedBy", entity.InsertedBy);

                var result = await command.ExecuteScalarAsync();
                if (result == null)
                    return -1;
                return Convert.ToInt32(result);
            }
        }
    }
}
