using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Administration.Entities;

public class MenuItem : ComponentEntity
{
    public HrefEntity Sidebar
    {
        get => new()
{
Href = string.Format("/api/menus/{0}/sidebars", this.ID)
};
    }        
}
