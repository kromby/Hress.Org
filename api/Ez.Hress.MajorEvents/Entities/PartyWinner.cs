using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.MajorEvents.Entities
{
    public class PartyWinner : UserBasicEntity
    {
        public PartyWinner(int id, string username, int count)
        {
            ID = id;
            Username = username;
            WinCount = count;
        }

        public int WinCount { get; set; }
    }
}
