using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class HardheadRuleFunctions
{
    private const string _class = nameof(HardheadRuleFunctions);

    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly RuleInteractor _ruleInteractor;
    private readonly PostElectionInteractor _postElectionInteractor;

    public HardheadRuleFunctions(AuthenticationInteractor authenticationInteractor, RuleInteractor ruleInteractor, PostElectionInteractor postElectionInteractor)
    {
        _ruleInteractor = ruleInteractor;
        _authenticationInteractor = authenticationInteractor;
        _postElectionInteractor = postElectionInteractor;
    }

    [FunctionName("hardheadRules")]
    public async Task<IActionResult> RunRules([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/rules/{id:int?}")] HttpRequest req, int? id, ILogger log)
    {
        const string method = nameof(RunRules);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);
        log.LogInformation("[{Class}.{Method}] Host: {Host}", _class, method, req.Host.Value);

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

    [FunctionName("hardheadRuleChanges")]
    public async Task<IActionResult> RunRuleChanges(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "hardhead/rules/changes")] HttpRequest req,
        ILogger log)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        log.LogInformation("[HardheadRuleFunctions] C# HTTP trigger function processed a request.");
        log.LogInformation($"[HardheadRuleFunctions] Host: {req.Host.Value}");

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);
        if (!isJWTValid)
        {
            log.LogInformation("[HardheadRuleFunctions] JWT is not valid!");
            return new UnauthorizedResult();
        }

        try
        {
            if (HttpMethods.IsPost(req.Method))
            {
                return await PostRuleChange(req, userID, log);
            }

            if (HttpMethods.IsGet(req.Method))
            {
                if (int.TryParse(req.Query["type"], out int typeID))
                    return await GetRuleChanges(typeID, log);

                return await GetRuleChanges(log);
            }

            log.LogError($"[HardheadRuleFunctions] HttpMethod '{req.Method}' ist not yet supported.");
            return new NotFoundResult();
        }
        catch (ArgumentException aex)
        {
            log.LogError(aex, "[HardheadRuleFunctions] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "[HardheadRuleFunctions] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            log.LogInformation($"[HardheadRuleFunctions] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [FunctionName("hardheadRuleIDChanges")]
    public async Task<IActionResult> RunRuleIDChanges([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/rules/{id:int}/changes")] HttpRequest req, int id, ILogger log)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        log.LogInformation("[{Class}.{Method}] Host: {Host}", _class, "RunRuleIDChanges", req.Host.Value);

        try
        {
            var list = await _ruleInteractor.GetRuleChangesByRuleAsync(id);
            return list.Count > 0 ? new OkObjectResult(list) : new NotFoundResult();
        }
        catch (ArgumentException aex)
        {
            log.LogError(aex, "[HardheadRuleFunctions] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "[HardheadRuleFunctions] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            log.LogInformation($"[HardheadRuleFunctions] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [FunctionName("hardheadRulePostElection")]
    public async Task<IActionResult> RulePostElection([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/rules/postelection")] HttpRequest req, ILogger log)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, nameof(RulePostElection));

        try
        {
            var result = await _postElectionInteractor.UpdateRulesAsync();
            return new OkObjectResult(result);
        }
        catch (ArgumentException aex)
        {
            log.LogError(aex, "[HardheadRuleFunctions] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "[HardheadRuleFunctions] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            log.LogInformation($"[HardheadRuleFunctions] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    private async Task<IActionResult> GetRuleChanges(int typeID, ILogger log)
    {
        log.LogInformation("[HardheadRuleFunctions] GetRuleChanges typeID: {typeID}", typeID);

        var result = await _ruleInteractor.GetRuleChangesAsync(typeID);

        return new OkObjectResult(result);
    }

    private async Task<IActionResult> GetRuleChanges(ILogger log)
    {
        log.LogInformation("[HardheadRuleFunctions] GetRuleChanges");

        var result = await _ruleInteractor.GetRuleChangesAsync();

        return new OkObjectResult(result);
    }

    private async Task<IActionResult> PostRuleChange(HttpRequest req, int userID, ILogger log)
    {
        log.LogInformation("[HardheadRuleFunctions] PostAwardNominations");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        RuleChange change = JsonConvert.DeserializeObject<RuleChange>(requestBody);
        change.InsertedBy = userID;

        log.LogInformation($"[HardheadRuleFunctions] Request body: {requestBody}");

        await _ruleInteractor.SubmitRuleChangeAsync(change);
        return new NoContentResult();
    }
}
