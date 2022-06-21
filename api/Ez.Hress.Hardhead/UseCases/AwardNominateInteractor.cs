using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Hardhead.UseCases
{
    public class AwardNominateInteractor
    {
        private IAwardNominateDataAccess _awardDataAccess;
        private ILogger<AwardNominateInteractor> _log;
        private IUserInteractor _userInteractor;

        public AwardNominateInteractor(IAwardNominateDataAccess dataAccess, IUserInteractor userInteractor, ILogger<AwardNominateInteractor> log)
        {
            _awardDataAccess = dataAccess;
            _userInteractor = userInteractor;
            _log = log;            
        }

        public async Task<int> Nominate(Nomination nomination)
        {
            if (nomination == null)
                throw new ArgumentNullException(nameof(nomination));            

            nomination.Validate();
            nomination.CreatedDate = DateTime.Now;

            nomination.Nominee = await _userInteractor.GetUser(nomination.Nominee.ID);

            _log.LogInformation($"[{nameof(AwardNominateInteractor)}] Nominating {nomination.Nominee.Username} in group {nomination.TypeID} for {nomination.Description} by {nomination.CreatedBy}");

            var result = await _awardDataAccess.SaveNomination(nomination);
            return result;
        }
    }
}