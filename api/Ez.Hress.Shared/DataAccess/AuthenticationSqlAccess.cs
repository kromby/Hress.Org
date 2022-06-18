using Ez.Hress.Shared.UseCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.DataAccess
{
    public class AuthenticationSqlAccess : IAuthenticationDataAccess
    {
        public Task<int> GetUserID(string username, string hashedPassword)
        {
            throw new NotImplementedException();
        }

        public Task SaveLoginInformation(int userId, string ipAddress)
        {
            throw new NotImplementedException();
        }
    }
}
