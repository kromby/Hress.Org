using Ez.Hress.Hardhead.Entities;
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
        private readonly IAwardDataAccess _awardDataAccess;
        private readonly ILogger<HardheadElectionInteractor> _log;

        public HardheadElectionInteractor(IAwardDataAccess awardDataAccess, IHardheadDataAccess hardheadDataAccess, ElectionInteractor electionInteractor, ILogger<HardheadElectionInteractor> log)
        {
            _awardDataAccess= awardDataAccess;
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

                if (voter.LastElectionID == votingYearID)
                    return null;

                awardList.Insert(0, new Award() { ID = 100, Name = "Lög Harðhausa - Nýjar og niðurfelldar reglur" });
                awardList.Insert(1, new Award() { ID = 101, Name = "Lög Harðhausa - Reglubreytingar" });
                awardList.Insert(2, new Award() { ID = 102, Name = "Kjósa myndir fyrir janúar kvöldið" });

                return awardList.Where(a => a.ID != 363 && a.ID > voter.LastStepID).FirstOrDefault();
            }
            else
            {
                return awardList.Where(a => a.ID == 361 || a.ID == 362).FirstOrDefault();
            }
        }
    }
}
