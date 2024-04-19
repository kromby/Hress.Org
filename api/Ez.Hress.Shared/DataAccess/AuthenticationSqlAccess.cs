using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Ez.Hress.Shared.DataAccess;

public class AuthenticationSqlAccess : IAuthenticationDataAccess
{
    private readonly ILogger<AuthenticationSqlAccess> _log;
    private readonly string _connectionString;

    public AuthenticationSqlAccess(DbConnectionInfo connectionInfo, ILogger<AuthenticationSqlAccess> log)
    {
        _connectionString = connectionInfo.ConnectionString;
        _log = log;
    }

    public async Task<int> GetUserID(string username, string hashedPassword)
    {
        _log.LogInformation("[{Class}] Getting user id for '{Username}'", nameof(AuthenticationSqlAccess), username);

        var sql = "SELECT Id, ApiPassword FROM adm_User WHERE Username = @username";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var apiPassword = reader.GetString(1);
            if (apiPassword == hashedPassword)
            {
                return reader.GetInt32(0);
            }
        }

        return -1;
    }

    public async Task<int> GetUserID(string magicCode)
    {
        var sql = @"SELECT	magic.InsertedBy
                        FROM	adm_MagicCode magic
                        WHERE	magic.Code = @code
	                        AND magic.Deleted > GetDate()
	                        AND magic.DeletedBy IS NULL";

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("code", magicCode));
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task SaveLoginInformation(int userId, string ipAddress)
    {
        var sql = @"UPDATE adm_User
                SET LastLoggedIn = GetDate(),
                    LastLoggedInBy = @ipAddress,
                    LoginCount = LoginCount + 1,
                    IsOnline = 1
            WHERE Id = @userID";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userID", userId);
        command.Parameters.AddWithValue("@ipAddress", ipAddress);

        _ = await command.ExecuteNonQueryAsync();
    }

    public async Task<int> SaveMagicCode(int userID, string code, DateTime expires)
    {
        var sql = @"INSERT INTO [dbo].[adm_MagicCode]([Code],[InsertedBy],[Deleted])
                        VALUES (@code, @insertedBy, @deleted)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("code", code));
        command.Parameters.Add(new SqlParameter("insertedBy", userID));
        command.Parameters.Add(new SqlParameter("deleted", expires));

        return await command.ExecuteNonQueryAsync();
    }

    public async Task SavePassword(int userID, string hashedPassword)
    {
        _log.LogInformation("[{Class}] Changing password for user '{userID}'", nameof(AuthenticationSqlAccess), userID);
        var sql = "UPDATE [adm_User] SET [ApiPassword] = @password WHERE Id = @userID";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@userID", userID);
        command.Parameters.AddWithValue("@password", hashedPassword);

        int affected = await command.ExecuteNonQueryAsync();

        if(affected == 0)
        {
            _log.LogError("[{Class}] User '{UserID}' not found", nameof(AuthenticationSqlAccess), userID);
            throw new SystemException("User not found");
        }
    }

    public async Task<bool> VerifyPassword(int userID, string hashedPassword)
    {
        _log.LogInformation("[{Class}] Verifying password for user '{userID}'", nameof(AuthenticationSqlAccess), userID);

        var sql = "SELECT ApiPassword FROM adm_User WHERE Id = @userID";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userID", userID);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var apiPassword = reader.GetString(0);
            return (apiPassword == hashedPassword);
        }

        return false;
    }
}
