using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using System.Data.SqlClient;

namespace Ez.Hress.Shared.DataAccess;

public class ConfigurationSqlAccess : IConfigurationDataAccess
{
    private readonly string _connectionString;

    public ConfigurationSqlAccess(DbConnectionInfo connectionInfo)
    {
        _connectionString = connectionInfo.ConnectionString;
    }

    public async Task<IDictionary<string, string>> GetConfiguration()
    {
        var sql = @"SELECT	config.Id, config.ConfigKey, Config.ConfigValue
                        FROM	sys_Configuration config
                        WHERE	config.Deleted IS NULL";

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand(sql, connection);

        var reader = await command.ExecuteReaderAsync();
        IDictionary<string, string> list = new Dictionary<string, string>();    
        while(reader.Read())
        {
            list.Add(reader.GetString(reader.GetOrdinal("ConfigKey")), reader.GetString(reader.GetOrdinal("ConfigValue")));
        }

        return list;
    }
}
