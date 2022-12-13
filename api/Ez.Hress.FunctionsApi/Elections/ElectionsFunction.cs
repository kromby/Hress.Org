using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Shared.UseCases;
using Ez.Hress.Hardhead.UseCases;

namespace Ez.Hress.FunctionsApi.Elections
{
    public class ElectionsFunction
    {
        private readonly AuthenticationInteractor _authenticationInteractor;
        private readonly HardheadElectionInteractor _hardheadElectionInteractor;
        public ElectionsFunction(AuthenticationInteractor authenticationInteractor, HardheadElectionInteractor hardheadElectionInteractor)
        {
            _authenticationInteractor = authenticationInteractor;
            _hardheadElectionInteractor = hardheadElectionInteractor;
        }

        [FunctionName("electionsAccess")]
        public async Task<IActionResult> RunAccess(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "elections/{electionID:int}/voters/access")] HttpRequest req,
            int electionID, ILogger log)
        {
            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAccess));

            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, out int userID, log);
            if (!isJWTValid)
            {
                log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunAccess));
                return new UnauthorizedResult();
            }

            var result = await _hardheadElectionInteractor.CheckAccess(userID);
            if (result != null)
                return new OkObjectResult(result);
            else
                return new NotFoundResult();
        }
    }
}
