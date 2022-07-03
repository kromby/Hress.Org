using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Administration.Entities
{
    public class MenuItem : ComponentEntity
    {
        public HrefEntity Sidebar
        {
            get
            {
                return new HrefEntity()
                {
                    Href = string.Format("/api/menus/{0}/sidebars", this.ID)
                };
            }
        }        
    }
}
