using Ez.Hress.Shared.UseCases;
using Ez.Hress.UserProfile.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Users;

public class UsersFunction
{
    private readonly string _function = nameof(UsersFunction);
    private readonly UserProfileInteractor _userProfileInteractor;
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly ILogger<UsersFunction> _log;

    public UsersFunction(AuthenticationInteractor authenticationInteractor, UserProfileInteractor userProfileInteractor, ILogger<UsersFunction> log)
    {
        _userProfileInteractor = userProfileInteractor;
        _authenticationInteractor = authenticationInteractor;
        _log = log;
    }

    [Function("users")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id:int}")] HttpRequest req, int id)
    {
        var methodName = nameof(Run);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            var entity = await _userProfileInteractor.GetUserAsync(id);
            return entity == null ? new NotFoundResult() : new OkObjectResult(entity);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _function, methodName);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _function, methodName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[HardheadRuleFunctions] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [Function("usersBalanceSheet")]
    public async Task<IActionResult> RunBalanceSheet(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id:int}/balancesheet")] HttpRequest req,
        int id)
    {
        var methodName = nameof(RunBalanceSheet);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        if (!AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID))
        {
            _log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunBalanceSheet));
            return new UnauthorizedResult();
        }

        var myId = id != 0 ? id : userID;

        bool includePaid = false;
        if(req.Query.ContainsKey("includePaid"))
        {
            bool.TryParse(req.Query["includePaid"], out includePaid);
        }

        var entity = await _userProfileInteractor.GetBalanceSheetAsync(myId, includePaid);
        return new OkObjectResult(entity);
    }

    [Function("userPassword")]
    public async Task<IActionResult> RunPassword([HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/{id:int}/password")] HttpRequest req, int id)
    {
        var methodName = nameof(RunPassword);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        if (!AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID))
        {
            _log.LogInformation("[{Class}.{Method}] JWT is not valid!", _function, methodName);
            return new UnauthorizedResult();
        }

        id = id != 0 ? id : userID;
        if (userID != id)
        {
            _log.LogWarning("[{Class}.{Method}] Don't have access to that user!", _function, methodName);
            return new UnauthorizedObjectResult("Don't have access to that user!");
        }

        try
        {
            var body = await req.ReadFromJsonAsync<ChangePasswordBody>();
            await _authenticationInteractor.ChangePasswordAsync(id, body.Password, body.NewPassword);
            return new AcceptedResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _function, methodName);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (UnauthorizedAccessException uaex)
        {
            _log.LogError(uaex, "[{Class}.{Method}] Unauthorized", _function, methodName);
            return new UnauthorizedResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _function, methodName);
            throw;
        }
    }
}
