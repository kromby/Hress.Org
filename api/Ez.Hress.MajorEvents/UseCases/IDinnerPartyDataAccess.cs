using Ez.Hress.MajorEvents.Entities;
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

        Task<IList<Guest>> GetGuests(int partyID, int? typeID);

        Task<IList<Course>> GetCoursesByTypeId(int typeID);

        Task SaveVote(Vote vote);
    }
}
