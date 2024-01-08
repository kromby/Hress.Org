using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Configuration;
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
        private readonly IAwardDataAccess _awardDataAccess;
        private readonly ILogger<HardheadElectionInteractor> _log;
        private readonly IConfiguration _config;

        public HardheadElectionInteractor(IAwardDataAccess awardDataAccess, IHardheadDataAccess hardheadDataAccess, ElectionInteractor electionInteractor, IConfiguration config, ILogger<HardheadElectionInteractor> log)
        {
            _awardDataAccess= awardDataAccess;
            _electionInteractor = electionInteractor;
            _hardheadDataAccess= hardheadDataAccess;
            _config = config;
            _log = log;
        }

        public async Task<Award?> CheckAccess(int userID)
        {
            _log.LogInformation("[{Method}] Checking access for user: {userID}", nameof(CheckAccess), userID);
            _ = int.TryParse(_config["Hress.Monthly.REP_HEAD.AppsForVoter"], out int requiredNightCount);
            _ = int.TryParse(_config["Hress.Monthly.REP_HEAD.AppsForCenturionVoter"], out int requiredNightCountForCenturion);
            _ = int.TryParse(_config["Hress.Monthly.REP_HEAD.TwentyYearID"], out int twentyYearOldID);
            if (!int.TryParse(_config["Hress.Monthly.REP_HEAD.Year"], out int votingYearID))
            {
                _log.LogError("Can't read Election YEAR from config");
                throw new SystemException("Can't read Election YEAR from config");
            }

            var awardListTask = _awardDataAccess.GetAwards(null);
            var userList = await _hardheadDataAccess.GetHardheadUsers(votingYearID);            
            var user = userList.Where(u => u.ID == userID).FirstOrDefault();

            if (user == null)
                return null;
              
            var voter = await _electionInteractor.GetVoter(userID);

            if (voter == null || voter.LastElectionID == votingYearID)
                return null;

            int lastStepID = voter.LastStepID ?? 0;

            awardListTask.Wait();
            var awardList = awardListTask.Result;

            awardList.First(a => a.ID == 361).Href = $"/api/hardhead?parentID={votingYearID}";
            awardList.First(a => a.ID == 362).Href = $"/api/hardhead?parentID={votingYearID}";
            awardList.First(a => a.ID == 364).Href = $"/api/hardhead/{votingYearID}/users?attended=8";

            if (user.Attended >= requiredNightCount)
            {
                return GetNextElectionStep(lastStepID, twentyYearOldID, awardList);
            }
            else if(user.Attended >= requiredNightCountForCenturion)
            {
                // TODO: Check if voter is Centurion
                if(userID == 2665 || userID == 2637 || userID == 2635)
                {
                    return GetNextElectionStep(lastStepID, twentyYearOldID, awardList);
                }
            }
            
            return awardList.Where(a => (a.ID == 361 || a.ID == 362) && a.ID > voter.LastStepID).FirstOrDefault();
        }

        private static Award? GetNextElectionStep(int lastStepID, int twentyYearOldID, IList<Award> awardList)
        {
            awardList.Insert(0, new Award() { ID = 100, Name = "Nýjar reglur" });
            awardList.Insert(1, new Award() { ID = 101, Name = "Reglubreytingar" });
            awardList.Insert(2, item: new Award() { ID = 102, Name = "Mynd á uppgjörskvöld", Href=$"/api/hardhead?parentID={twentyYearOldID}" });

            return awardList.FirstOrDefault(a => a.ID != 363 && a.ID > lastStepID);
        }

        public async Task SaveVoter(int userID, int electionID)
        {
            var voter = new VoterEntity()
            {
                ID = userID,
                LastStepID = electionID
            };
            await _electionInteractor.SaveVoter(voter);
        }

        public async Task<bool> SaveVote(VoteEntity entity, int userID)
        {
            if (entity == null)
                throw new ArgumentException("Entity is missing.", nameof(entity));

            entity.Validate();

            var result = await _electionInteractor.SaveVote(entity);
            if (!result)
                return result;

            await SaveVoter(userID, entity.StepID);

            return result;
        }

        public async Task<int> SaveVotes(IList<VoteEntity> list, int stepID, int userID)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("Entity is missing.", nameof(list));

            int result = 0;
            foreach (var vote in list)
            {
                vote.StepID = stepID;
                vote.Validate();

                if(await _electionInteractor.SaveVote(vote))
                    result++;
            }

            if (result == 0)
                return 0;

            await SaveVoter(userID, stepID);

            return result;
        }
    }
}
