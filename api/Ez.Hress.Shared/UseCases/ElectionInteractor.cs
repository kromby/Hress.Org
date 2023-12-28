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
        private readonly IElectionVoterDataAccess _electionVoterDataAccess;
        private readonly IElectionVoteDataAccess _voteDataAccess;

        public ElectionInteractor(IElectionVoterDataAccess voterVataAccess, IElectionVoteDataAccess voteDataAccess)
        {
            _electionVoterDataAccess = voterVataAccess;
            _voteDataAccess = voteDataAccess;
        }

        public Task<VoterEntity?> GetVoter(int userID)
        {
            return _electionVoterDataAccess.GetVoter(userID);
        }

        public Task<int> SaveVoter(VoterEntity voter)
        {
            return _electionVoterDataAccess.SaveVoter(voter);
        }

        public async Task<bool> SaveVote(VoteEntity vote)
        {
            if (vote == null)
                throw new ArgumentException("Entity is missing.", nameof(vote));

            vote.Validate();
            return await _voteDataAccess.SaveVote(vote);
        }
    }
}
