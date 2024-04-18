using Ez.Hress.Administration.Entities;

namespace Ez.Hress.Administration.UseCases;

public interface IMenuDataAccess
{
    Task<IList<MenuItem>> GetMenuItems(int id, bool includePrivate);

    Task<IList<MenuItem>> GetMenuItems(string navigateUrl, bool includePrivate);

    //IList<SidebarItem> GetSidebarItems(int parentId);
}
