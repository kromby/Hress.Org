using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public interface IHardheadElectionDataAccess
    {
        Task<int> SaveVote(Vote entity);
    }
}
