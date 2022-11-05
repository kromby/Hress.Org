using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.Entities
{
    public class TypeEntity : EntityBase<int>
    {
        public TypeEntity(int id, string name, string code)
        {
            ID = id;
            Name = name;
            Code = code;
        }

        public string? Description { get; set; }
        public string Code { get; set; }
    }
}
