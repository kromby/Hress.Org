using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.UseCases
{
    public class ElectionInteractor
    {
        private readonly IElectionDataAccess electionDataAccess;

        public ElectionInteractor(IElectionDataAccess dataAccess)
        {
            electionDataAccess = dataAccess;
        }

        public Task<VoterEntity?> GetVoter(int userID)
        {
            return electionDataAccess.GetVoter(userID);
        }

        public Task<int> SaveVoter(VoterEntity voter)
        {
            return electionDataAccess.SaveVoter(voter);
        }
    }
}
