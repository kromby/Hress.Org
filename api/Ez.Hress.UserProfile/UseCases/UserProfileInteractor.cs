using Ez.Hress.Shared.Entities;
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

        public async Task<UserBasicEntity> GetUser(int userID)
        {
            return await _userDataAccess.GetUser(userID);
        }

        public async Task<BalanceSheet> GetBalanceSheet(int userID)
        {
            _log.LogInformation("[{Class}] GetBalanceSheet", GetType().Name);

            var transactionTask = _userDataAccess.GetTransactions(userID);

            var relations = await _userDataAccess.GetRelations(userID);
            var entity = new BalanceSheet() { UserID = userID };

            transactionTask.Wait();
            entity.Transactions = transactionTask.Result;

            foreach (var relation in relations)
            {                
                var transactions = await _userDataAccess.GetTransactions(relation.RelatedUser.ID);
                entity.Transactions = entity.Transactions.Union(transactions).ToList();
            }

            if (entity.Transactions.Count > 0)
            {
                entity.Transactions = entity.Transactions.OrderBy(t => t.Inserted).ToList();
                entity.Balance = entity.Transactions.Sum(t => t.Amount);
            }

            return entity;
        }
    }
}
