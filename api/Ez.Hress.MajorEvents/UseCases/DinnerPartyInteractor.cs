using Ez.Hress.MajorEvents.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.MajorEvents.UseCases
{
    public class DinnerPartyInteractor
    {
        private readonly IDinnerPartyDataAccess _dinnerDataAccess;
        private readonly ILogger<DinnerPartyInteractor> _log;

        public DinnerPartyInteractor(IDinnerPartyDataAccess dinnerDataAccess, ILogger<DinnerPartyInteractor> logger)
        {
            _dinnerDataAccess = dinnerDataAccess;
            _log = logger;
        }
    
        public async Task<IList<DinnerParty>> GetDinnerParties()
        {
            _log.LogInformation("[{Class}] GetDinnerParties", GetType().Name);
            var list = await _dinnerDataAccess.GetAll();

            foreach(var item in list)
            {
                item.Guests = await _dinnerDataAccess.GetGuests(item.ID, 197);
            }

            return list;
        }

        public async Task<DinnerParty> GetDinnerParty(int id)
        {
            return await _dinnerDataAccess.GetById(id);
        }

        public async void GetGuests(int id)
        {
            throw new NotImplementedException();
        }

        public async void GetCourses(int id)
        {
            throw new NotImplementedException();
        }

        public async void GetRedwineTeams(int id)
        {
            throw new NotImplementedException();
        }

        public async void GetAlbums(int id)
        {
            throw new NotImplementedException();
        }
    }
}
