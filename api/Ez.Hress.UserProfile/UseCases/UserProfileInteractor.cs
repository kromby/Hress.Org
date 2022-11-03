using Ez.Hress.UserProfile.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.UserProfile.UseCases
{
    public class UserProfileInteractor
    {
        private readonly IUserProfileDataAccess _userDataAccess;
        private readonly ILogger<UserProfileInteractor> _log;

        public UserProfileInteractor(IUserProfileDataAccess userDataAccess, ILogger<UserProfileInteractor> log)
        {
            _userDataAccess = userDataAccess;
            _log = log;
        }

        public async Task<BalanceSheet> GetBalanceSheet(int userID)
        {
            // Add check for JWT
            // Should this only work for the ID in JWT or not?


            _log.LogInformation("[{Class}] GetBalanceSheet", GetType().Name);

            var entity = new BalanceSheet() { UserID = userID };
            entity.Transactions = await _userDataAccess.GetTransactions(userID);
            if (entity.Transactions.Count > 0)
            {
                entity.Balance = entity.Transactions.Sum(t => t.Amount);
            }

            return entity;
        }
    }
}
