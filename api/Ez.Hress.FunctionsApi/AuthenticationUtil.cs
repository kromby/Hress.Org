using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.FunctionsApi
{
    internal class AuthenticationUtil
    {
        private const string AUTHORIZATION_HEADER_NAME = "X-Custom-Authorization";

        public static bool GetAuthenticatedUserID(AuthenticationInteractor authenticationInteractor, IHeaderDictionary headers, out int userID, ILogger log)
        {

            
            if(!headers.ContainsKey(AUTHORIZATION_HEADER_NAME))
            {
                log.LogInformation($"[GetAuthenticatedUserID] AuthorisationHeader is missing.");
                userID = -1;
                return false;
            }

            log.LogInformation($"[GetAuthenticatedUserID] {AUTHORIZATION_HEADER_NAME} header: '{headers[AUTHORIZATION_HEADER_NAME].ToString()}'");

            var authorizationHeader = headers[AUTHORIZATION_HEADER_NAME].ToString().Split(" ");

            var authInfo = authenticationInteractor.GetUserIdFromToken(authorizationHeader[0], authorizationHeader[1]);
            userID = authInfo.Item2;

            return authInfo.Item1;
        }
    }
}
