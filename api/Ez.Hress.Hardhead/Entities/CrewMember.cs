using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities;

public class CrewMember: EntityBase<int>
{
    public CrewMember()
    {
        Name = string.Empty;    
    }

    public Role Role { get; set; }

    public int MovieCounter { get; set; }
}

public enum Role
{
    Director,
    Writer,
    Actor
}
