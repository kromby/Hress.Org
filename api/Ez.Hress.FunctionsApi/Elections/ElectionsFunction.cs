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
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Net;
using Ez.Hress.Shared.Entities;
using System.Collections.Generic;
using System.Linq;
using Ez.Hress.Hardhead.Entities;

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

        [FunctionName("electionsVote")]
        public async Task<IActionResult> RunVote([HttpTrigger(AuthorizationLevel.Function, "post", Route = "elections/{id:int}/vote")] HttpRequest req, int id, ILogger log)
        {
            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunVote));

            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, out int userID, log);
            if (!isJWTValid)
            {
                log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunVote));
                return new UnauthorizedResult();
            }
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var voteList = JsonConvert.DeserializeObject<IList<Vote>>(requestBody);

                if (voteList.Count == 1)
                {
                    var entity = voteList.First();
                    if (entity.EventID == 0)
                        entity.EventID = id;

                    var result = await _hardheadElectionInteractor.SaveVote(entity, userID).ConfigureAwait(false);
                    return result > 0 ? new OkResult() : throw new SystemException("Could not save vote.");
                }
                else
                {
                    var result = await _hardheadElectionInteractor.SaveVotes(voteList, id, userID).ConfigureAwait(false);
                    return result > 0 ? new OkResult() : throw new SystemException("Could not save vote.");
                }
            }
            catch (ArgumentException aex)
            {
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
