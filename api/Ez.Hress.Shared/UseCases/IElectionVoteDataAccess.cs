using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.UseCases
{
    public interface IElectionVoteDataAccess
    {
        Task<bool> SaveVote(VoteEntity vote);

        Task<IList<VoteEntity>> GetVotes(Guid stepID);
    }
}
