using Ez.Hress.Administration.Entities;
using Ez.Hress.Administration.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Ez.Hress.Administration.DataAccess;

public class MenuSqlDataAccess : IMenuDataAccess
{
    private readonly ILogger<MenuSqlDataAccess> _log;
    private readonly string _connectionString;
    public MenuSqlDataAccess(DbConnectionInfo connectionInfo, ILogger<MenuSqlDataAccess> log)
    {
        _connectionString = connectionInfo.ConnectionString;
        _log = log;
    }

    public async Task<IList<MenuItem>> GetMenuItems(string navigateUrl, bool includePrivate)
    {
        return await GetComponents<MenuItem>(navigateUrl, true, null, "MENU", includePrivate);
    }

    public async Task<IList<MenuItem>> GetMenuItems(int parentId, bool includePrivate)
    {
        return await GetComponents<MenuItem>(string.Empty, false, parentId, "MENU", includePrivate);
    }

    //public async Task<IList<SidebarItem>> GetSidebarItems(int parentId)
    //{
    //    return await GetComponents<SidebarItem>(string.Empty, false, parentId, "SIDEBAR");
    //}


    #region Private methods


    private async Task<IList<T>> GetComponents<T>(string navigateUrl, bool parentIdRequired, int? parentId, string typeCode, bool includePrivate)
    {
        _log.LogInformation("[{Class}] Getting components {NavigateUrl}", nameof(MenuSqlDataAccess), navigateUrl);

        var sql = @"SELECT	component.Id, component.Name, component.Description, component.NavigateUrl, component.IsPublic, component.IsLegacy, componentType.Shortcode
                        FROM	adm_Component component
                        JOIN	gen_Type componentType ON component.TypeId = componentType.Id
                        WHERE	component.Deleted IS NULL	                        
                            AND	componentType.Shortcode = @componentType";

        if(!includePrivate)
        {
            sql += " AND component.IsPublic = 1";
        }

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand();
        command.Connection = connection;

        command.Parameters.Add(new SqlParameter("componentType", typeCode));

        if (parentIdRequired)
        {
            sql += " AND component.ParentId IS NOT NULL ";
        }

        if (!string.IsNullOrEmpty(navigateUrl))
        {
            sql += " AND component.NavigateUrl = @navigateUrl ";
            command.Parameters.Add(new SqlParameter("navigateUrl", navigateUrl));
        }

        if (parentId.HasValue)
        {
            sql += " AND component.ParentId = @parentId ";
            command.Parameters.Add(new SqlParameter("parentId", parentId.Value));
        }

        command.CommandText = sql + " ORDER BY component.Sort";

        var list = new List<T>();

        var reader = await command.ExecuteReaderAsync();
        while (reader.Read())
        {
            T? component = (T?)Activator.CreateInstance(typeof(T));
            if (component != null)
            {
                ParseComponent<T>(reader, component);
                list.Add(component);
            }
        }

        return list;
    }

    private static void ParseComponent<T>(SqlDataReader reader, T item)
    {
        if (item is not ComponentEntity entity)
            return;

        entity.ID = reader.GetInt32(reader.GetOrdinal("Id"));
        entity.Name = reader.GetString(reader.GetOrdinal("Name"));
        entity.Description = reader.GetString(reader.GetOrdinal("Description"));
        entity.Link = new HrefEntity()
        {
            Href = reader.GetString(reader.GetOrdinal("NavigateUrl"))
        };
        entity.Public = reader.GetBoolean(reader.GetOrdinal("IsPublic"));
        entity.IsLegacy = reader.GetBoolean(reader.GetOrdinal("IsLegacy"));
    }


    #endregion
}
