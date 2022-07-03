using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.Entities
{
    public class ComponentEntity : EntityBase<int>
    {
        public string? Description { get; set; }
        public HrefEntity? Link { get; set; }
        public bool Public { get; set; }
        public bool IsLegacy { get; set; }
    }
}
