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

        public Task<VoterEntity> GetVoter(int userID)
        {
            return electionDataAccess.GetVoter(userID);
        }

        //public Task<int> SaveVoter(VoterEntity voter)
        //{
        //    return electionDataAccess.SaveVoter(voter);
        //}

        //public async Task<int> SaveVote(VoteEntity entity, int userID)
        //{
        //    if (entity == null)
        //        throw new ArgumentException("Entity is missing.", nameof(entity));

        //    entity.Created = DateTime.Now;
        //    entity.Validate();

        //    var result = await electionDataAccess.SaveVote(entity).ConfigureAwait(false);

        //    var voter = new VoterEntity()
        //    {
        //        ID = userID,
        //        LastStepID = entity.EventID
        //    };
        //    await SaveVoter(voter);

        //    return result;
        //}

        //public async Task<int> SaveVotes(IList<VoteEntity> list, int electionID, int userID)
        //{
        //    if (list == null || list.Count == 0)
        //        throw new ArgumentException("Entity is missing.", nameof(list));

        //    int result = 0;
        //    foreach (var vote in list)
        //    {
        //        vote.EventID = electionID;
        //        vote.Created = DateTime.Now;
        //        vote.Validate();

        //        result += await electionDataAccess.SaveVote(vote).ConfigureAwait(false);
        //    }

        //    var voter = new VoterEntity()
        //    {
        //        ID = userID,
        //        LastStepID = electionID
        //    };
        //    await SaveVoter(voter);

        //    return result;
        //}
    }
}
