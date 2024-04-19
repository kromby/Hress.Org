using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Hardhead.Entities;
using System.Diagnostics;
using Ez.Hress.Shared.UseCases;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class HardheadAwardFunctions
{
    private readonly AwardNominateInteractor _awardWriteInteractor;
    private readonly AwardNominationInteractor _awardReadInteractor;
    private readonly AuthenticationInteractor _authenticationInteractor;

    public HardheadAwardFunctions(AuthenticationInteractor authenticationInteractor, AwardNominateInteractor awardWriteInteractor, AwardNominationInteractor awardReadInteractor)
    {
        _authenticationInteractor = authenticationInteractor;
        _awardWriteInteractor = awardWriteInteractor;
        _awardReadInteractor = awardReadInteractor;
    }

    [FunctionName("hardheadAwardsNominations")]
    public async Task<IActionResult> RunAwardNominations(
    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "hardhead/awards/nominations")] HttpRequest req,
    ILogger log)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        log.LogInformation("[RunAwardNominations] C# HTTP trigger function processed a request.");
        log.LogInformation($"[RunAwardNominations] Host: {req.Host.Value}");

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);
        if (!isJWTValid)
        {
            log.LogInformation("[RunAwardNominations] JWT is not valid!");
            return new UnauthorizedResult();
        }

        try
        {
            if (HttpMethods.IsGet(req.Method))
                return await GetAwardNominations(req, userID, log);
            if (HttpMethods.IsPost(req.Method))
                return await PostAwardNominations(req, userID, log);

            log.LogError($"[RunAwardNominations] HttpMethod '{req.Method}' ist not yet supported.");
            return new NotFoundResult();
        }
        catch (ArgumentException aex)
        {
            log.LogError(aex, "[RunAwardNominations] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "[RunAwardNominations] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            log.LogInformation($"[RunAwardNominations] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    private async Task<IActionResult> PostAwardNominations(HttpRequest req, int userID, ILogger log)
    {
        log.LogInformation("[RunAwardNominations] PostAwardNominations");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Nomination nom = JsonConvert.DeserializeObject<Nomination>(requestBody);
        nom.InsertedBy = userID;

        log.LogInformation($"[RunAwardNominations] Request body: {requestBody}");

        await _awardWriteInteractor.NominateAsync(nom);
        return new NoContentResult();
    }

    private async Task<IActionResult> GetAwardNominations(HttpRequest req, int excludeUserID, ILogger log)
    {
        log.LogInformation("[RunAwardNominations] GetAwardNominations");

        if (!req.Query.ContainsKey("type"))
        {
            throw new ArgumentNullException(nameof(req), "Type query parameter is required.");
        }

        if (!int.TryParse(req.Query["type"], out int typeID))
        {
            throw new ArgumentException("Type query parameter is not a valid integer.");
        }

        var list = await _awardReadInteractor.GetNominationsAsync(typeID, excludeUserID);

        if (list.Count == 0)
            return new NotFoundResult();

        return new OkObjectResult(list);
    }
}
