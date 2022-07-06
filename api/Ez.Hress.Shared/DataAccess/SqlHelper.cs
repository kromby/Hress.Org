using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.DataAccess
{
    public static class SqlHelper
    {
        public static int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            int columnIndex = reader.GetOrdinal(columnName);
            return (reader.IsDBNull(columnIndex) ? new int?() : reader.GetInt32(columnIndex));
        }
        public static DateTime GetDateTime(SqlDataReader reader, string columnName)
        {
            int columnIndex = reader.GetOrdinal(columnName);
            return (reader.IsDBNull(columnIndex)) ? new DateTime() : reader.GetDateTime(columnIndex);
        }
        public static DateTime? GetDateTimeNullable(SqlDataReader reader, string columnName)
        {
            int columnIndex = reader.GetOrdinal(columnName);
            return (reader.IsDBNull(columnIndex)) ? new DateTime?() : reader.GetDateTime(columnIndex);
        }
    }
}
