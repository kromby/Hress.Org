using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities
{
    public class StatisticUserEntity : StatisticBase
    {
        public StatisticUserEntity()
        {
            User = new UserBasicEntity();
        }

        public UserBasicEntity User { get; set; }
    }
}