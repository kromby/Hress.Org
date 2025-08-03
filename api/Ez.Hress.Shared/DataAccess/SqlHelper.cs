using Microsoft.Data.SqlClient;

namespace Ez.Hress.Shared.DataAccess;

public static class SqlHelper
{

    public static bool GetBool(System.Data.SqlClient.SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return (!reader.IsDBNull(columnIndex) && reader.GetBoolean(columnIndex)); 
    }

    public static int GetInt(System.Data.SqlClient.SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return (reader.IsDBNull(columnIndex) ? -1 : reader.GetInt32(columnIndex));
    }        
    public static int? GetNullableInt(System.Data.SqlClient.SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return (reader.IsDBNull(columnIndex) ? new int?() : reader.GetInt32(columnIndex));
    }

    public static int? GetNullableInt(SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return (reader.IsDBNull(columnIndex) ? new int?() : reader.GetInt32(columnIndex));
    }

    public static int? GetNullableDecimalToInt(System.Data.SqlClient.SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return (reader.IsDBNull(columnIndex) ? new int?() : Convert.ToInt32(reader.GetDecimal(columnIndex)));
    }

    public static DateTime GetDateTime(System.Data.SqlClient.SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return (reader.IsDBNull(columnIndex)) ? new DateTime() : reader.GetDateTime(columnIndex);
    }
    public static DateTime? GetDateTimeNullable(System.Data.SqlClient.SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return (reader.IsDBNull(columnIndex)) ? new DateTime?() : reader.GetDateTime(columnIndex);
    }
}
