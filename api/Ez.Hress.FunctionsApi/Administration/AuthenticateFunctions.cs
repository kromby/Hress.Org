using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using Ez.Hress.Shared.UseCases;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Administration;

public class AuthenticateFunctions
{
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly ILogger<AuthenticateFunctions> _log;
    private readonly string _class = nameof(AuthenticateFunctions);

    public AuthenticateFunctions(AuthenticationInteractor authenticationInteractor, ILogger<AuthenticateFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _log = log;
    }

    [Function("authenticate")]
    public async Task<IActionResult> RunAuthenticate(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[{Class}.RunAuthenticate] C# HTTP trigger function processed a request.", _class);
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        AuthenticateBody body = JsonConvert.DeserializeObject<AuthenticateBody>(requestBody);
        _log.LogInformation("[{Class}.RunAuthenticate] Request IP Address: {IPAddress}", _class, req.HttpContext.Connection.RemoteIpAddress.MapToIPv4());

        try
        {
            if (!string.IsNullOrWhiteSpace(body.Username))
            {
                _log.LogInformation("[{Class}.RunAuthenticate] Authenticate by username: {Username}", _class, body.Username);
                var jwt = await _authenticationInteractor.LoginAsync(body.Username, body.Password, req.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString());
                return new OkObjectResult(jwt);
            }
            
            if (!string.IsNullOrWhiteSpace(body.Code))
            {
                _log.LogInformation("[{Class}.RunAuthenticate] Authenticate by code", _class, body.Username);
                var jwt = await _authenticationInteractor.LoginAsync(body.Code, req.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString());
                return new OkObjectResult(jwt);
            }

            _log.LogWarning("[{Class}.RunAuthenticate] Authenticate function processed a request with no username or code.", _class);
            return new UnauthorizedResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.RunAuthenticate] Invalid input", _class);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (UnauthorizedAccessException uaex)
        {
            _log.LogError(uaex, "[{Class}.RunAuthenticate] Unauthorized", _class);
            return new UnauthorizedResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.RunAuthenticate] Unhandled error", _class);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Class}.RunAuthenticate] Elapsed: {Elapsed} ms.", _class, stopwatch.ElapsedMilliseconds);
        }
    }

    [Function("authenticateMagic")]
    public async Task<IActionResult> RunMagic([HttpTrigger(AuthorizationLevel.Function, "post", Route = "authenticate/magic")] HttpRequest req)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[RunMagic] C# HTTP trigger function processed a request.");

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[RunMagic] JWT is not valid!");
            return new UnauthorizedResult();
        }

        try
        {
            var code = await _authenticationInteractor.CreateMagicCodeAsync(userID);
            return new OkObjectResult(code);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[RunMagic] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (UnauthorizedAccessException uaex)
        {
            _log.LogError(uaex, "[RunMagic] Unauthorized");
            return new UnauthorizedResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[RunMagic] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[RunMagic] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
