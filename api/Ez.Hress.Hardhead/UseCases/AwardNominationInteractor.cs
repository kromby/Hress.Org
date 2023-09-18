using Ez.Hress.Hardhead.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public class AwardNominationInteractor
    {
        private readonly IAwardNominationDataAccess _awardDataAccess;
        private readonly ILogger<AwardNominationInteractor> _log;

        public AwardNominationInteractor(IAwardNominationDataAccess dataAccess, ILogger<AwardNominationInteractor> log)
        {
            _awardDataAccess = dataAccess;
            _log = log;
        }

        public async Task<IList<Nomination>> GetNominations(int typeID, int excludedUserID)
        {
            _log.LogInformation("[{Class}] Getting nominations for type {typeID}", nameof(AwardNominationInteractor), typeID);

            var list = await _awardDataAccess.GetNominations(typeID);

            return list.Where(x => x.Nominee.ID != excludedUserID).ToList();
        }
    }
}
