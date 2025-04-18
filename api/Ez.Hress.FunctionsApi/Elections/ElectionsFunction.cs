using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Shared.UseCases;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Elections;

public class ElectionsFunction
{
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly HardheadElectionInteractor _hardheadElectionInteractor;
    private readonly ILogger<ElectionsFunction> _log;
    public ElectionsFunction(AuthenticationInteractor authenticationInteractor, HardheadElectionInteractor hardheadElectionInteractor, ILogger<ElectionsFunction> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _hardheadElectionInteractor = hardheadElectionInteractor;
        _log = log;
    }

    [Function("electionsAccess")]
    public async Task<IActionResult> RunAccess(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "elections/{electionID:int}/voters/access")] HttpRequest req,
        int electionID)
    {
        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAccess));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunAccess));
            return new UnauthorizedResult();
        }

        var result = await _hardheadElectionInteractor.CheckAccessAsync(userID);
        if (result != null)
            return new OkObjectResult(result);

        return new NotFoundResult();
    }

    [Function("electionsVote")]
    public async Task<IActionResult> RunVote([HttpTrigger(AuthorizationLevel.Function, "post", Route = "elections/{id:int}/vote")] HttpRequest req, int id)
    {
        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunVote));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunVote));
            return new UnauthorizedResult();
        }
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var voteList = JsonConvert.DeserializeObject<IList<VoteEntity>>(requestBody);

            if (voteList.Count == 0)
            {
                await _hardheadElectionInteractor.SaveVoterAsync(userID, id);
                return new OkResult();
            }

            if (voteList.Count == 1)
            {
                var entity = voteList.First();
                if (entity.StepID == 0)
                    entity.StepID = id;

                var voteResult = await _hardheadElectionInteractor.SaveVoteAsync(entity, userID).ConfigureAwait(false);
                return voteResult ? new OkResult() : throw new SystemException("Could not save vote.");
            }

            var result = await _hardheadElectionInteractor.SaveVotesAsync(voteList, id, userID).ConfigureAwait(false);
            return result > 0 ? new OkResult() : throw new SystemException("Could not save vote.");
        }
        catch (ArgumentException aex)
        {
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, ex.Message);
            throw;
        }
    }
}
