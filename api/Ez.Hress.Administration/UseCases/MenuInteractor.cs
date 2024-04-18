using Ez.Hress.Administration.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Administration.UseCases;

public class MenuInteractor
{
    private readonly IMenuDataAccess _menuDataAccess;
    private readonly ILogger<MenuInteractor> _log;

    public MenuInteractor(IMenuDataAccess menuDataAccess, ILogger<MenuInteractor> log)
    {
        _menuDataAccess = menuDataAccess;
        _log = log;
    }

    public async Task<IList<MenuItem>> GetMenuItems(string navigateUrl, int userID, bool fetchChildren)
    {
        if (navigateUrl.LastIndexOf("/") > 2)
            navigateUrl = navigateUrl[..navigateUrl.IndexOf("/", 3)];

        if (navigateUrl.ToLower().Equals("~/default"))
            navigateUrl = "~/";

        bool includePrivate = userID > 0;

        _log.LogInformation($"[{nameof(MenuInteractor)}] navigateUrl: '{navigateUrl}', includePrivate: '{includePrivate}', fetchChildren: '{fetchChildren}'");

        var list = await _menuDataAccess.GetMenuItems(navigateUrl, includePrivate);

        if (fetchChildren && list.Count > 0)
        {
            list = await _menuDataAccess.GetMenuItems(list.First().ID, includePrivate);

            foreach (var item in list)
            {
                if (item.Link?.Href != null)
                {
                    item.Link.Href = item.IsLegacy ? item.Link.Href.Replace("~", "") : item.Link.Href.Replace("~", "").Replace(".aspx", "");
                }                     
            }
        }

        return list;
    }

    public async Task<IList<MenuItem>> GetMenuItems(int parentID, int userID)
    {
        bool includePrive = userID > 0;
        return await _menuDataAccess.GetMenuItems(parentID, includePrive);
    }

    public async Task<IList<MenuItem>> GetMenuRoot(int userID)
    {
        bool includePrive = userID > 0;
        var list = await _menuDataAccess.GetMenuItems(34322, includePrive);

        foreach (var item in list)
        {
            if (item.Link?.Href != null)
            {
                item.Link.Href = item.IsLegacy ? item.Link.Href.Replace("~", "") : item.Link.Href.Replace("~", "").Replace(".aspx", "");
            }
        }

        return list.Where(i => i.ID != 28755).ToList();
    }

    //public IList<SidebarItem> GetSidebarItems(int parentId)
    //{
    //    return await _menuDataAccess.GetSidebarItems(parentId);
    //}
}