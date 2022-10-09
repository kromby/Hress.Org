using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.MajorEvents.Entities
{
    public class PartyTeam : EntityBase<int>
    {
        public PartyTeam(int id, int number, bool isWinner)
        {
            ID = id;
            Number = number;
            IsWinner = isWinner;

            Members = new List<PartyUser>();
        }

        public int Number { get; set; }

        public string? Wine { get; set; }

        public bool IsWinner { get; set; }

        public IList<PartyUser> Members { get; set; }
    }
}
