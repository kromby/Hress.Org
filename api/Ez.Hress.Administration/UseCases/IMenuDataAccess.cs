using Ez.Hress.Administration.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Administration.UseCases
{
    public interface IMenuDataAccess
    {
        Task<IList<MenuItem>> GetMenuItems(int id, bool includePrivate);

        Task<IList<MenuItem>> GetMenuItems(string navigateUrl, bool includePrivate);

        //IList<SidebarItem> GetSidebarItems(int parentId);
    }
}
