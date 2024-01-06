using Ez.Hress.MajorEvents.Entities;
using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.MajorEvents.UseCases
{
    public interface IDinnerPartyDataAccess
    {
        Task<IList<DinnerParty>> GetAll();
        Task<DinnerParty?> GetById(int id);

        Task<IList<PartyUser>> GetGuests(int partyID, int? typeID);

        Task<IList<Course>> GetCourses(int partyID);
        
        Task<IList<Dish>> GetCoursesByTypeId(int typeID);

        Task<IList<NameHrefEntity<int>>> GetAlbums(int partyID);

        Task<IList<PartyTeam>> GetChilds(int partyID);

        Task<IList<PartyUser>> GetChildUsers(int partyID);

        Task<IList<PartyQuiz>> GetQuizQuestions(int teamID);

        Task<IList<PartyWinner>> GetWinnerStatistic();

        Task SaveVote(Vote vote);
    }
}
