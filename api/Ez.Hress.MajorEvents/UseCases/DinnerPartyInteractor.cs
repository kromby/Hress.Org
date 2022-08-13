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

                if(item.Guests.Count > 1 && item.Guests.First().ID == 2663)
                    item.Guests = item.Guests.OrderBy(item => item.ID).ToList();
            }

            return list;
        }

        public async Task<DinnerParty> GetDinnerParty(int id)
        {
            return await _dinnerDataAccess.GetById(id);
        }

        //public async void GetGuests(int id)
        //{
        //    throw new NotImplementedException();
        //}

        //public async void GetCourses(int id)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<IList<Course>> GetCoursesByType(int typeID)
        {
            return await _dinnerDataAccess.GetCoursesByTypeId(typeID);
        }

        //public async void GetRedwineTeams(int id)
        //{
        //    throw new NotImplementedException();
        //}

        //public async void GetAlbums(int id)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task SaveVote(Vote vote)
        {
            await _dinnerDataAccess.SaveVote(vote);
        }
    }
}
