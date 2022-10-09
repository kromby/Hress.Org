using Ez.Hress.MajorEvents.Entities;
using Microsoft.Extensions.Logging;

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

        public async Task<DinnerParty?> GetDinnerParty(int id)
        {
            var dinnerPartyTask = _dinnerDataAccess.GetById(id);
            var guestsTask = _dinnerDataAccess.GetGuests(id, null);

            var dinnerParty = await dinnerPartyTask;
            guestsTask.Wait();

            if (dinnerParty != null)
            {                
                dinnerParty.Guests = guestsTask.Result;
            }

            return dinnerParty;
        }

        //public async void GetGuests(int id)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<IList<Course>> GetCourses(int partyID)
        {
            return await _dinnerDataAccess.GetCourses(partyID);
        }

        public async Task<IList<Dish>> GetCoursesByType(int typeID)
        {
            return await _dinnerDataAccess.GetCoursesByTypeId(typeID);
        }

        public async Task<IList<PartyTeam>> GetRedwineTeams(int partyID)
        {            
            var userThread = _dinnerDataAccess.GetChildUsers(partyID);
            var teams = await _dinnerDataAccess.GetChilds(partyID);
            userThread.Wait();
            var users = userThread.Result;

            foreach(var team in teams)
            {
                team.Members = users.Where(m => m.ParentID == team.ID).ToList();
            }

            return teams;
        }

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
