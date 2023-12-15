using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public class HardheadInteractor
    {
        private readonly IHardheadDataAccess _hardheadDataAccess;
        private readonly ILogger<HardheadInteractor> _log;
        private readonly string _class = nameof(HardheadInteractor);
        public HardheadInteractor(IHardheadDataAccess hardheadDataAccess, ILogger<HardheadInteractor> log)
        {
            _hardheadDataAccess = hardheadDataAccess;
            _log = log;
        }

        public async Task<HardheadNight> GetHardhead(int id)
        {
            _log.LogInformation("[{Class}] Getting Hardhead '{ID}'", _class, id);
            return await _hardheadDataAccess.GetHardhead(id);
        }

        public async Task<IList<HardheadNight>> GetHardheads(DateTime fromDate)
        {
            _log.LogInformation("[{Class}] Getting all Hardheads from '{from}' until now", _class, fromDate);
            return await _hardheadDataAccess.GetHardheads(fromDate, DateTime.Now.AddDays(-1));
        }

        public async Task<IList<HardheadNight>> GetHardheads(int parentID)
        {
            _log.LogInformation("[{Class}] Getting all Hardheads for parent '{ParentID}'", _class, parentID);
            return await _hardheadDataAccess.GetHardheads(parentID);
        }

        public async Task<IList<HardheadNight>> GetNextHardhead()
        {
            var list = await _hardheadDataAccess.GetHardheads(DateTime.Now.AddDays(-1), DateTime.Now.AddMonths(2));
            return list;
        }

        public async Task<IList<HardheadNight>> GetHardheads(int userID, UserType type)
        {
            var idList = await _hardheadDataAccess.GetHardheadIDsByHostOrGuest(userID, type).ConfigureAwait(false);

            if (idList != null && idList.Any())
            {
                return await _hardheadDataAccess.GetHardheads(idList).ConfigureAwait(false);
            }
            return new List<HardheadNight>();
        }

        public async Task SaveHardhead(HardheadNight night, int userID)
        {
            night.Validate();

            var oldNight = await _hardheadDataAccess.GetHardhead(night.ID);
            if (oldNight.Date.Year != night.Date.Year || oldNight.Date.Month != night.Date.Month)
            {
                throw new ArgumentException("Ekki má breyta ári eða mánuði harðhausakvölds.", nameof(night));
            }

            // It's handled in the stored procedure to do nothing when description is empty.
            if (night.Description == oldNight.Description)
                night.Description = string.Empty;

            night.Updated = DateTime.Now;
            night.UpdatedBy = userID;

            if (!await _hardheadDataAccess.AlterHardhead(night))
                throw new SystemException("Saving failed");

            if (night.NextHostID.HasValue && night.NextHostID.Value != night.Host.ID)
            {
                var tempDate = night.Date.AddMonths(1);
                var nextFirstDate = new DateTime(tempDate.Year, tempDate.Month, 1);
                var nextDate = new DateTime(tempDate.Year, tempDate.Month, DateTime.DaysInMonth(tempDate.Year, tempDate.Month));

                var nextHardheadList = await _hardheadDataAccess.GetHardheads(nextFirstDate, nextDate);

                if (nextHardheadList.Count == 0)
                {
                    if (!await _hardheadDataAccess.CreateHardhead(night.NextHostID.Value, nextDate, userID, DateTime.Now))
                        throw new SystemException("Creating new hardhead failed");
                }
            }
        }

        public async Task<IList<ComponentEntity>> GetActions(int hardheadID, int userID)
        {
            var actionsTask = _hardheadDataAccess.GetActions(hardheadID);
            var hardhead = await _hardheadDataAccess.GetHardhead(hardheadID).ConfigureAwait(false);
            if (userID == hardhead.Host.ID || userID == 2630)
            {
                actionsTask.Wait();
                return actionsTask.Result;
            }

            return new List<ComponentEntity>();

            //Some ideas regarding implementation
            //To start with the only action will be change the hardhead
            //Future actions could be "I was there", "Tilnefna vonbrigði"...
        }
    }
}
