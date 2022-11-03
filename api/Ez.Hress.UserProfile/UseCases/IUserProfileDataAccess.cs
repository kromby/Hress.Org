using Ez.Hress.UserProfile.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.UserProfile.UseCases
{
    public interface IUserProfileDataAccess
    {
        Task<IList<Transaction>> GetTransactions(int userID);
    }
}
