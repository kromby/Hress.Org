using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Ez.Hress.Shared.DataAccess;

public class UserSqlDataAccess : IUserDataAccess
{
    private readonly ILogger<UserSqlDataAccess> _log;
    private readonly string _connectionString;

    public UserSqlDataAccess(DbConnectionInfo connectionInfo, ILogger<UserSqlDataAccess> log)
    {
        _connectionString = connectionInfo.ConnectionString;
        _log = log;
    }
    
    public async Task<UserBasicEntity> GetUser(int id)
    {
        var sql = @"SELECT	usr.Id, usr.Username, usr.Inserted, uimg.ImageId, tName.TextValue 'Name'
                        FROM adm_User usr
                        LEFT JOIN upf_Image uimg ON usr.Id = uimg.UserId AND uimg.TypeId = 14
                        LEFT JOIN upf_Text tName ON tName.UserId = usr.Id AND tName.TypeId = 83
                        WHERE usr.Id = @userID";
        _log.LogInformation("[{Class}.{Method}] id: {ID}", nameof(UserSqlDataAccess), nameof(GetUser), id);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("userID", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return ReadUserBasicEntity(reader);
        }

        return new UserBasicEntity();
    }

    public async Task<IList<UserBasicEntity>> GetUsers()
    {
        const string sql = @"SELECT	usr.Id, usr.Username, usr.Inserted, uimg.ImageId, tName.TextValue 'Name'
                                FROM adm_User usr
                                LEFT JOIN upf_Image uimg ON usr.Id = uimg.UserId AND uimg.TypeId = 14
                                LEFT JOIN upf_Text tName ON tName.UserId = usr.Id AND tName.TypeId = 83
                                WHERE usr.Deleted IS NULL";

        _log.LogInformation("[{Class}.{Method}] Getting all users", nameof(UserSqlDataAccess), nameof(GetUsers));

        return await ExecuteUserQuery(sql);
    }

    public async Task<IList<UserBasicEntity>> GetUsersByRole(string roleCode)
    {
        const string sql = @"SELECT	usr.Id, usr.Username, usr.Inserted, uimg.ImageId, tName.TextValue 'Name'
                                FROM adm_User usr
                                LEFT JOIN upf_Image uimg ON usr.Id = uimg.UserId AND uimg.TypeId = 14
                                LEFT JOIN upf_Text tName ON tName.UserId = usr.Id AND tName.TypeId = 83
                                JOIN upf_Lookup uRole ON usr.Id = uRole.UserId AND uRole.TypeId = 107
                                WHERE usr.Deleted IS NULL
                                AND uRole.ValueId = @roleId";

        int roleId = roleCode switch
        {
            "US_L_HRESS" => 98,
            "US_L_FRND" => 99,
            "US_L_HEAD" => 108,
            _ => 0
        };

        _log.LogInformation("[{Class}.{Method}] roleCode: {RoleCode}, roleId: {RoleId}", nameof(UserSqlDataAccess), nameof(GetUsersByRole), roleCode, roleId);

        return await ExecuteUserQuery(sql, new SqlParameter("roleId", roleId));
    }

    private async Task<IList<UserBasicEntity>> ExecuteUserQuery(string sql, SqlParameter? parameter = null)
    {
        var list = new List<UserBasicEntity>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        if (parameter != null)
            command.Parameters.Add(parameter);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(ReadUserBasicEntity(reader));
        }

        return list;
    }

    private static UserBasicEntity ReadUserBasicEntity(System.Data.Common.DbDataReader reader)
    {
        var user = new UserBasicEntity
        {
            ID = reader.GetInt32(reader.GetOrdinal("Id")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            Inserted = reader.GetDateTime(reader.GetOrdinal("Inserted"))
        };

        if (!reader.IsDBNull(reader.GetOrdinal("ImageId")))
            user.ProfilePhotoId = reader.GetInt32(reader.GetOrdinal("ImageId"));

        if (!reader.IsDBNull(reader.GetOrdinal("Name")))
            user.Name = reader.GetString(reader.GetOrdinal("Name"));

        return user;
    }
}
