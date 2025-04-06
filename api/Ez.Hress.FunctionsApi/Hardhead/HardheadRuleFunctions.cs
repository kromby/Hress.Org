using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class HardheadRuleFunctions
{
    private const string _class = nameof(HardheadRuleFunctions);

    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly RuleInteractor _ruleInteractor;
    private readonly PostElectionInteractor _postElectionInteractor;
    private readonly ILogger<HardheadRuleFunctions> _log;

    public HardheadRuleFunctions(AuthenticationInteractor authenticationInteractor, RuleInteractor ruleInteractor, PostElectionInteractor postElectionInteractor, ILogger<HardheadRuleFunctions> log)
    {
        _ruleInteractor = ruleInteractor;
        _authenticationInteractor = authenticationInteractor;
        _postElectionInteractor = postElectionInteractor;
        _log = log;
    }

    [Function("hardheadRules")]
    public async Task<IActionResult> RunRules([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/rules/{id:int?}")] HttpRequest req, int? id)
    {
        const string method = nameof(RunRules);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);
        _log.LogInformation("[{Class}.{Method}] Host: {Host}", _class, method, req.Host.Value);

        if (id.HasValue)
        {
            var childList = await _ruleInteractor.GetRulesAsync(id.Value);
            if (childList.Count <= 0)
                return new NotFoundResult();
            return new OkObjectResult(childList);
        }

        var parentList = await _ruleInteractor.GetRulesAsync();
        if (parentList.Count <= 0)
            return new NotFoundResult();
        return new OkObjectResult(parentList);
    }

    [Function("hardheadRuleChanges")]
    public async Task<IActionResult> RunRuleChanges(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "hardhead/rules/changes")] HttpRequest req)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[HardheadRuleFunctions] C# HTTP trigger function processed a request.");
        _log.LogInformation($"[HardheadRuleFunctions] Host: {req.Host.Value}");

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[HardheadRuleFunctions] JWT is not valid!");
            return new UnauthorizedResult();
        }

        try
        {
            if (HttpMethods.IsPost(req.Method))
            {
                return await PostRuleChange(req, userID);
            }

            if (HttpMethods.IsGet(req.Method))
            {
                if (int.TryParse(req.Query["type"], out int typeID))
                    return await GetRuleChanges(typeID);

                return await GetRuleChanges();
            }

            _log.LogError($"[HardheadRuleFunctions] HttpMethod '{req.Method}' ist not yet supported.");
            return new NotFoundResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[HardheadRuleFunctions] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[HardheadRuleFunctions] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[HardheadRuleFunctions] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [Function("hardheadRuleIDChanges")]
    public async Task<IActionResult> RunRuleIDChanges([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/rules/{id:int}/changes")] HttpRequest req, int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[{Class}.{Method}] Host: {Host}", _class, "RunRuleIDChanges", req.Host.Value);

        try
        {
            var list = await _ruleInteractor.GetRuleChangesByRuleAsync(id);
            return list.Count > 0 ? new OkObjectResult(list) : new NotFoundResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[HardheadRuleFunctions] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[HardheadRuleFunctions] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[HardheadRuleFunctions] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [Function("hardheadRulePostElection")]
    public async Task<IActionResult> RulePostElection([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/rules/postelection")] HttpRequest req)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, nameof(RulePostElection));

        try
        {
            var result = await _postElectionInteractor.UpdateRulesAsync();
            return new OkObjectResult(result);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[HardheadRuleFunctions] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[HardheadRuleFunctions] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[HardheadRuleFunctions] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    private async Task<IActionResult> GetRuleChanges(int typeID)
    {
        _log.LogInformation("[HardheadRuleFunctions] GetRuleChanges typeID: {typeID}", typeID);

        var result = await _ruleInteractor.GetRuleChangesAsync(typeID);

        return new OkObjectResult(result);
    }

    private async Task<IActionResult> GetRuleChanges()
    {
        _log.LogInformation("[HardheadRuleFunctions] GetRuleChanges");

        var result = await _ruleInteractor.GetRuleChangesAsync();

        return new OkObjectResult(result);
    }

    private async Task<IActionResult> PostRuleChange(HttpRequest req, int userID)
    {
        _log.LogInformation("[HardheadRuleFunctions] PostRuleChange");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        RuleChange change = JsonConvert.DeserializeObject<RuleChange>(requestBody);
        change.InsertedBy = userID;

        _log.LogInformation($"[HardheadRuleFunctions] Request body: {requestBody}");

        await _ruleInteractor.SubmitRuleChangeAsync(change);
        return new NoContentResult();
    }
}
