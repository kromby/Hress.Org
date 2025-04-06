using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Hardhead.Entities;
using System.Diagnostics;
using Ez.Hress.Shared.UseCases;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class HardheadAwardFunctions
{
    private readonly AwardNominateInteractor _awardWriteInteractor;
    private readonly AwardNominationInteractor _awardReadInteractor;
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly HardheadAwardInteractor _hardheadAwardInteractor;
    private readonly ILogger<HardheadAwardFunctions> _log;

        public HardheadAwardFunctions(
            AuthenticationInteractor authenticationInteractor, 
        AwardNominateInteractor awardWriteInteractor, 
        AwardNominationInteractor awardReadInteractor,
        HardheadAwardInteractor hardheadAwardInteractor,
        ILogger<HardheadAwardFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _awardWriteInteractor = awardWriteInteractor;
        _awardReadInteractor = awardReadInteractor;
        _hardheadAwardInteractor = hardheadAwardInteractor;
        _log = log;
    }

    [Function("hardheadAwardsNominations")]
    public async Task<IActionResult> RunAwardNominations(
    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "hardhead/awards/nominations")] HttpRequest req)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[RunAwardNominations] C# HTTP trigger function processed a request.");
        _log.LogInformation($"[RunAwardNominations] Host: {req.Host.Value}");

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[RunAwardNominations] JWT is not valid!");
            return new UnauthorizedResult();
        }

        try
        {
            if (HttpMethods.IsGet(req.Method))
                return await GetAwardNominations(req, userID);
            if (HttpMethods.IsPost(req.Method))
                return await PostAwardNominations(req, userID);

            _log.LogError($"[RunAwardNominations] HttpMethod '{req.Method}' ist not yet supported.");
            return new NotFoundResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[RunAwardNominations] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[RunAwardNominations] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[RunAwardNominations] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    private async Task<IActionResult> PostAwardNominations(HttpRequest req, int userID)
    {
        _log.LogInformation("[RunAwardNominations] PostAwardNominations");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Nomination nom = JsonConvert.DeserializeObject<Nomination>(requestBody);
        nom.InsertedBy = userID;

        _log.LogInformation($"[RunAwardNominations] Request body: {requestBody}");

        await _awardWriteInteractor.NominateAsync(nom);
        return new NoContentResult();
    }

    private async Task<IActionResult> GetAwardNominations(HttpRequest req, int excludeUserID)
    {
        _log.LogInformation("[RunAwardNominations] GetAwardNominations");

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

    [Function("hardheadAward")]
    public async Task<IActionResult> RunGetAward(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/awards/{id}")] HttpRequest req,
        string id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[GetAward] C# HTTP trigger function processed a request.");
        _log.LogInformation($"[GetAward] Host: {req.Host.Value}");

        try
        {
            // Special case: if id is "nominations", redirect to the nominations endpoint
            if (id == "nominations")
            {
                return await RunAwardNominations(req);
            }

            // Try to parse id as an integer and get a specific award
            if (!int.TryParse(id, out int awardId))
            {
                _log.LogWarning("[GetAward] Invalid award ID format: {ID}", id);
                return new BadRequestObjectResult("Invalid award ID format");
            }

            var award = await _hardheadAwardInteractor.GetAwardAsync(awardId);
            if (award == null)
            {
                _log.LogWarning("[GetAward] Award not found: {ID}", awardId);
                return new NotFoundResult();
            }

            _log.LogInformation("[GetAward] Request completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
            return new OkObjectResult(award);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[GetAward] Error processing request");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[GetAward] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [Function("hardheadAwardWinners")]
    public async Task<IActionResult> RunGetAwardWinners(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/awards/{id}/winners")] HttpRequest req,
        int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[GetAwardWinners] C# HTTP trigger function processed a request.");
        _log.LogInformation($"[GetAwardWinners] Host: {req.Host.Value}");

        try
        {
            // Parse optional year and position parameters
            int? year = null;
            if (req.Query.ContainsKey("year") && int.TryParse(req.Query["year"], out int tempYear))
            {
                year = tempYear;
            }

            int? position = null;
            if (req.Query.ContainsKey("position") && int.TryParse(req.Query["position"], out int tempPosition))
            {
                position = tempPosition;
            }

            int? userId = null;
            if (req.Query.ContainsKey("user") && int.TryParse(req.Query["user"], out int tempUser))
            {
                userId = tempUser;
            }

            // Call the interactor method to retrieve winners
            var winners = await _hardheadAwardInteractor.GetAwardWinnersAsync(id, year, position, userId);

            if (winners == null || winners.Count == 0)
            {
                _log.LogWarning("[GetAwardWinners] No winners found for award {AwardId}", id);
                return new NotFoundResult();
            }

            _log.LogInformation("[GetAwardWinners] Request completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
            return new OkObjectResult(winners);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[GetAwardWinners] Error processing request");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[GetAwardWinners] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [Function("hardheadAwards")]
    public async Task<IActionResult> RunGetAwards(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/awards")] HttpRequest req)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[GetAwards] C# HTTP trigger function processed a request.");
        _log.LogInformation($"[GetAwards] Host: {req.Host.Value}");

        try
        {
            int? year = null;
            if(req.Query.ContainsKey("year") && int.TryParse(req.Query["year"], out int tempYear))
            {
                year = tempYear;
            }

            var list = await _hardheadAwardInteractor.GetAwardsAsync(year);
            return new OkObjectResult(list);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[GetAwards] Error processing request");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[GetAwards] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
