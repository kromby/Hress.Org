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
    public class TypeSqlAccess : ITypeDataAccess
    {
        private readonly string _connectionString;
        private readonly ILogger<TypeSqlAccess> _log;
        private readonly string _class = nameof(TypeSqlAccess);

        public TypeSqlAccess(DbConnectionInfo connectionInfo, ILogger<TypeSqlAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }

        public async Task<IList<TypeEntity>> GetTypes()
        {
            string sql = @"SELECT	t.Id, t.Name, t.Description, t.ParentId, t.Shortcode, t.GroupType, t.Inserted, t.InsertedBy
                            FROM	gen_Type t
                            WHERE	t.Deleted IS NULL";
            _log.LogInformation("[{Class}] GetTypes sql: {SQL}", _class, sql);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);

            IList<TypeEntity> list = new List<TypeEntity>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while(await reader.ReadAsync())
                {
                    var entity = new TypeEntity(SqlHelper.GetInt(reader, "Id"))
                    {
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        ParentID = SqlHelper.GetNullableInt(reader, "ParentId"),
                        Code = reader.GetString(reader.GetOrdinal("Shortcode")),
                        GroupType = SqlHelper.GetInt(reader, "GroupType"),
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
