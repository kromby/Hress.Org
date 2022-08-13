using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.MajorEvents.Entities
{
    public class Vote : EntityBase<int>
    {
        public int TypeID { get; set; }

        public int CourseID { get; set; }
    }
}
