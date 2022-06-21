using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.UseCases
{
    public class UserInteractor : IUserInteractor
    {
        private IUserDataAccess _userDataAccess;
        private ILogger<UserInteractor> _log;

        public UserInteractor(IUserDataAccess userDataAccess, ILogger<UserInteractor> log)
        {
            _userDataAccess = userDataAccess;
            _log = log;
        }

        public async Task<UserBasicEntity> GetUser(int id)
        {
            _log.LogInformation($"[{nameof(UserInteractor)}] Getting user with id {0}", id);

            return await _userDataAccess.GetUser(id);
        }
    }
}
