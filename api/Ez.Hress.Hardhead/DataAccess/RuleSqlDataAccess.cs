using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess
{
    public class RuleSqlDataAccess : IRuleDataAccess
    {
        private const string _class = nameof(RuleSqlDataAccess);
        private readonly ILogger<RuleSqlDataAccess> _log;
        private readonly string _connectionString;

        public RuleSqlDataAccess(DbConnectionInfo connectionInfo, ILogger<RuleSqlDataAccess> log)
        {
            _connectionString = connectionInfo.ConnectionString;
            _log = log;
        }

        public async Task<IList<RuleParent>> GetRules()
        {
            var sql = @"SELECT	t.Id, t.TextValue, t.Inserted, t.InsertedBy
                        FROM	rep_Text t
                        JOIN	rep_Event e ON t.EventId = e.Id AND e.TypeId = 49
                        WHERE	t.TypeId = 70 AND t.ParentId is null AND t.Deleted IS NULL
                        ORDER BY Inserted";

            string method = nameof(GetRules);
            _log.LogInformation("[{Class}.{Method}] {SQL}", _class, method, sql);

            var list = new List<RuleParent>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using var command = new SqlCommand(sql, connection);
                var reader = await command.ExecuteReaderAsync();
                int i = 1;
                while (reader.Read())
                {
                    var entity = new RuleParent()
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("TextValue")),
                        Inserted = reader.GetDateTime(reader.GetOrdinal("Inserted")),
                        Number = i
                    };

                    list.Add(entity);
                    i++;
                }
            }

            return list;
        }

        public async Task<IList<RuleChild>> GetRules(int parentId)
        {
            var parentList = await GetRules();
            var parent = parentList.First(r => r.ID == parentId);


            var sql = @"SELECT	t.Id, t.TextValue, t.Inserted, t.InsertedBy
                        FROM	rep_Text t
                        WHERE	t.ParentId = @parentId AND t.TypeId = 70 AND t.Deleted IS NULL
						GROUP BY t.Id, t.TextValue, t.Inserted, t.InsertedBy
                        ORDER BY t.Inserted";

            string method = nameof(GetRules);
            _log.LogInformation("[{Class}.{Method}] {SQL}", _class, method, sql);

            var list = new List<RuleChild>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter("parentId", parentId));
                var reader = await command.ExecuteReaderAsync();
                int i = 1;
                while (reader.Read())
                {
                    var entity = new RuleChild()
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("TextValue")),
                        Inserted = reader.GetDateTime(reader.GetOrdinal("Inserted")),
                        Number = i,
                        ParentNumber = parent.Number
                    };

                    list.Add(entity);
                    i++;
                }
            }

            return list;
        }
    }
}
