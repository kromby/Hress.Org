using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.Entities
{
    public class VoterEntity : EntityBase<int>
    {
        public string? Username { get; set; }
        public int? LastElectionID { get; set; }
        public int? LastStepID { get; set; }
    }
}
