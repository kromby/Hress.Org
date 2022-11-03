using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.UserProfile.UseCases;
using Ez.Hress.Shared.UseCases;

namespace Ez.Hress.FunctionsApi.Users
{
    public class UsersFunction
    {
        private readonly UserProfileInteractor _userProfileInteractor;
        private readonly AuthenticationInteractor _authenticationInteractor;

        public UsersFunction(AuthenticationInteractor authenticationInteractor, UserProfileInteractor userProfileInteractor)
        {
            _userProfileInteractor = userProfileInteractor;
            _authenticationInteractor = authenticationInteractor;
        }

        [FunctionName("usersBalanceSheet")]
        public async Task<IActionResult> RunBalanceSheet(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id:int}/balancesheet")] HttpRequest req,
            int id, ILogger log)
        {
            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunBalanceSheet));

            if(!AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, out int userID, log))
            {
                log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunBalanceSheet));
                return new UnauthorizedResult();
            }

            var myId = id != 0 ? id : userID;
            var entity = await _userProfileInteractor.GetBalanceSheet(myId);
            return new OkObjectResult(entity);
        }
    }
}
