using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.UserProfile.Entities
{
    public class Transaction : EntityBase<int>
    {
        public int UserID { get; set; }
        public int Amount { get; set; }
    }
}
