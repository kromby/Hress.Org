using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.UseCases
{
    public interface IElectionDataAccess
    {
        Task<VoterEntity> GetVoter(int userID);

        //Task<int> SaveVoter(VoterEntity voter);

        //Task<int> SaveVote(VoteEntity entity);
    }
}
