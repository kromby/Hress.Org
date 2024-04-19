using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.FunctionsApi;

internal class AuthenticationUtil
{
    private const string AUTHORIZATION_HEADER_NAME = "X-Custom-Authorization";

    public static bool GetAuthenticatedUserID(AuthenticationInteractor authenticationInteractor, IHeaderDictionary headers, ILogger log, out int userID)
    {
        log.LogInformation($"[GetAuthenticatedUserID] AUTHORIZATION_HEADER_NAME: '{AUTHORIZATION_HEADER_NAME}'");
        if (!headers.ContainsKey(AUTHORIZATION_HEADER_NAME))
        {
            log.LogInformation("[GetAuthenticatedUserID] AuthorisationHeader is missing.");
            userID = -1;
            return false;
        }

        var authorizationHeader = headers[AUTHORIZATION_HEADER_NAME].ToString().Split(" ");
        
        log.LogInformation($"[GetAuthenticatedUserID] authorizationHeader[0]: '{authorizationHeader[0]}'");

        var authInfo = authenticationInteractor.GetUserIdFromToken(authorizationHeader[0], authorizationHeader[1]);
        userID = authInfo.Item2;

        return authInfo.Item1;
    }
}
