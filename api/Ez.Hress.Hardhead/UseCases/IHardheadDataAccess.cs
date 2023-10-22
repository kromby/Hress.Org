using Ez.Hress.Hardhead.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public interface IHardheadDataAccess
    {       
        Task<IList<HardheadUser>> GetHardheadUsers(int yearID);

        Task<HardheadNight> GetHardhead(int id);

        Task<IList<HardheadNight>> GetHardheads(DateTime fromDate, DateTime toDate);

        Task<IList<HardheadNight>> GetHardheads(int parentID);

        Task<IList<HardheadNight>> GetHardheads(IList<int> idList);

        Task<IList<int>> GetHardheadIDsByHostOrGuest(int userID, UserType type);

        Task<bool> AlterHardhead(HardheadNight hardhead);

        Task<bool> CreateHardhead(int hostID, DateTime nextDate, int currentUserID, DateTime changeDate);
    }
}
