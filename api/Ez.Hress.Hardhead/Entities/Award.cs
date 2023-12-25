using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities
{
    public class Award : EntityBase<int>
    {
        public HrefEntity Winners
        {
            get
            {
                return new HrefEntity()
                {
                    ID = this.ID,
                    Href = string.Format("/api/hardhead/awards/{0}/winners", this.ID)
                };
            }
        }

        public string? Href { get; set; }

        //public IList<YearEntity> Years { get; set; }
    }
}
