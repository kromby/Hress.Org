using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Hardhead.UseCases
{
    public class AwardNominateInteractor
    {
        private readonly IAwardNominateDataAccess _awardDataAccess;
        private readonly ILogger<AwardNominateInteractor> _log;
        private readonly IUserInteractor _userInteractor;

        public AwardNominateInteractor(IAwardNominateDataAccess dataAccess, IUserInteractor userInteractor, ILogger<AwardNominateInteractor> log)
        {
            _awardDataAccess = dataAccess;
            _userInteractor = userInteractor;
            _log = log;            
        }

        public async Task<int> NominateAsync(Nomination nomination)
        {
            if (nomination == null)
                throw new ArgumentNullException(nameof(nomination));            

            nomination.Validate();
            nomination.Inserted = DateTime.UtcNow;

            nomination.Nominee = await _userInteractor.GetUser(nomination.Nominee.ID);

            _log.LogInformation("[{Class}] Nominating {Username} in group {TypeID} for {Description} by {InsertedBy}", nameof(AwardNominateInteractor), nomination.Nominee.Username, nomination.TypeID, nomination.Description, nomination.InsertedBy);

            var result = await _awardDataAccess.SaveNomination(nomination);
            return result;
        }
    }
}