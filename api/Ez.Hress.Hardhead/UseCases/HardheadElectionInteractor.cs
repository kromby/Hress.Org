using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public class HardheadElectionInteractor
    {
        private readonly ElectionInteractor _electionInteractor;
        private readonly IHardheadDataAccess _hardheadDataAccess;
        private readonly IHardheadElectionDataAccess _hardheadElectionDataAccess;
        private readonly IAwardDataAccess _awardDataAccess;
        private readonly ILogger<HardheadElectionInteractor> _log;

        public HardheadElectionInteractor(IAwardDataAccess awardDataAccess, IHardheadDataAccess hardheadDataAccess, IHardheadElectionDataAccess hardheadElectionDataAccess, ElectionInteractor electionInteractor, ILogger<HardheadElectionInteractor> log)
        {
            _awardDataAccess= awardDataAccess;
            _hardheadElectionDataAccess = hardheadElectionDataAccess;
            _electionInteractor = electionInteractor;
            _hardheadDataAccess= hardheadDataAccess;
            _log = log;
        }

        public async Task<Award?> CheckAccess(int userID)
        {
            _log.LogInformation("[{Method}] Checking access for user: {userID}", nameof(CheckAccess), userID);
            // TODO: Read these two values from config or database
            int requiredNightCount = 6;
            int votingYearID = 5384; // 2022

            var awardListTask = _awardDataAccess.GetAwards(null);
            var userList = await _hardheadDataAccess.GetHardheadUsers(votingYearID);            
            var user = userList.Where(u => u.ID == userID).FirstOrDefault();

            if (user == null)
                return null;

            awardListTask.Wait();
            var awardList = awardListTask.Result;

            if (user.Attended >= requiredNightCount)
            {
                var voter = await _electionInteractor.GetVoter(userID);

                if (voter == null || voter.LastElectionID == votingYearID)
                    return null;

                awardList.Insert(0, new Award() { ID = 100, Name = "Nýjar og niðurfelldar reglur" });
                awardList.Insert(1, new Award() { ID = 101, Name = "Reglubreytingar" });
                awardList.Insert(2, new Award() { ID = 102, Name = "Mynd á uppgjörskvöld" });

                return awardList.Where(a => a.ID != 363 && a.ID > voter.LastStepID).FirstOrDefault();
            }
            else
            {
                return awardList.Where(a => a.ID == 361 || a.ID == 362).FirstOrDefault();
            }
        }

        public async Task<int> SaveVote(Vote entity, int userID)
        {
            if (entity == null)
                throw new ArgumentException("Entity is missing.", nameof(entity));

            entity.Created = DateTime.Now;
            entity.Validate();

            var result = await _hardheadElectionDataAccess.SaveVote(entity).ConfigureAwait(false);

            var voter = new VoterEntity()
            {
                ID = userID,
                LastStepID = entity.EventID
            };
            await _electionInteractor.SaveVoter(voter);

            return result;
        }

        public async Task<int> SaveVotes(IList<Vote> list, int electionID, int userID)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("Entity is missing.", nameof(list));

            int result = 0;
            foreach (var vote in list)
            {
                vote.EventID = electionID;
                vote.Created = DateTime.Now;
                vote.Validate();

                result += await _hardheadElectionDataAccess.SaveVote(vote).ConfigureAwait(false);
            }

            var voter = new VoterEntity()
            {
                ID = userID,
                LastStepID = electionID
            };
            await _electionInteractor.SaveVoter(voter);

            return result;
        }
    }
}
