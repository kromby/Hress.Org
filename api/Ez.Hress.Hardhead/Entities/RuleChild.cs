using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities
{
    public class RuleChild : EntityBase<int>
    {
        public HrefEntity? Changes
        {
            get
            {
                if (ChangeCount == 0)
                    return null;

                return new HrefEntity()
                {
                    Href = string.Format("/api/hardhead/rules/{0}/changes", ID)
                };
            }
        }

        public int ParentNumber { get; set; }
        public int Number { get; set; }
        public int ChangeCount { private get; set; }
    }
}
