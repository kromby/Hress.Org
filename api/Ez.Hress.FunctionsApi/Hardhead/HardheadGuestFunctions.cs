using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class HardheadGuestFunctions
{
    private readonly HardheadInteractor _hardheadInteractor;
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly ILogger<HardheadGuestFunctions> _log;
    private readonly string _class = nameof(HardheadRatingFunctions);

    public HardheadGuestFunctions(AuthenticationInteractor authenticationInteractor, HardheadInteractor hardheadInteractor, ILogger<HardheadGuestFunctions> log)
    {
        _authenticationInteractor  = authenticationInteractor;
        _hardheadInteractor = hardheadInteractor;
        _log = log;
    }

    [Function("hardheadGuests")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/{id:int}/guests")] HttpRequest req, int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, nameof(Run));

        try
        {
            var guests = await _hardheadInteractor.GetGuestsAsync(id);
            return new OkObjectResult(guests);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _class, nameof(Run));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _class, nameof(Run));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Class}.{Method}] Elapsed: {Elapsed} ms.", _class, nameof(Run), stopwatch.ElapsedMilliseconds);
        }
    }

    [Function("hardheadGuestPostDel")]
    public async Task<IActionResult> RunPostDel(
        [HttpTrigger(AuthorizationLevel.Function, "post", "delete", Route = "hardhead/{id:int}/guests/{guestId:int}")] HttpRequest req, int id, int guestId)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, nameof(RunPostDel));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userId);
        if (!isJWTValid)
        {
            _log.LogInformation("[HardheadRuleFunctions] JWT is not valid!");
            return new UnauthorizedResult();
        }

        try
        {
            if(HttpMethods.IsPost(req.Method))
            {
                var result = await _hardheadInteractor.AddGuestAsync(id, guestId, userId);
                if(result == 0)
                    return new NotFoundResult();
                return new OkResult();
            }

            if(HttpMethods.IsDelete(req.Method))
            {

            }

            _log.LogError($"[HardheadRuleFunctions] HttpMethod '{req.Method}' ist not yet supported.");
            return new NotFoundResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _class, nameof(Run));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _class, nameof(Run));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Class}.{Method}] Elapsed: {Elapsed} ms.", _class, nameof(Run), stopwatch.ElapsedMilliseconds);
        }
    }
}
