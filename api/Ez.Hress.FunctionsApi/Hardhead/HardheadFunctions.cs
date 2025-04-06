using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Shared.UseCases;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Hardhead.Entities;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class HardheadFunctions
{
    private readonly string _class = nameof(HardheadFunctions);
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly HardheadInteractor _hardheadInteractor;
    private readonly ILogger<HardheadFunctions> _log;

    public HardheadFunctions(AuthenticationInteractor authenticationInteractor, HardheadInteractor hardheadInteractor, ILogger<HardheadFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _hardheadInteractor = hardheadInteractor;
        _log = log;
    }

    [Function("hardhead")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "put", Route = "hardhead/{id:int?}")] HttpRequest req, int? id)
    {
        var method = nameof(Run);
        _log.LogInformation("[{Class}.{Function}] C# HTTP trigger function processed a request.", _class, method);

        if (HttpMethods.IsPut(req.Method))
        {
            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
            if (!isJWTValid)
            {
                _log.LogInformation("[{Class}.{Function}]  JWT is not valid!", _class, method);
                return new UnauthorizedResult();
            }

            try
            {
                HardheadNight night = await req.ReadFromJsonAsync<HardheadNight>();
                await _hardheadInteractor.SaveHardheadAsync(night, userID);
                return new OkResult();
            }
            catch (JsonReaderException jrex)
            {
                _log.LogWarning("[{Class}.{Function}] PUT - Can't parse request body, exception: {exception}", _class, method, jrex.Message);
                return new BadRequestResult();
            }
            catch (ArgumentException aex)
            {
                return new BadRequestObjectResult(aex.Message);
            }
        }
        else // GET
        {
            if (id.HasValue)
            {
                var entity = await _hardheadInteractor.GetHardheadAsync(id.Value);
                return new OkObjectResult(entity);
            }

            IList<HardheadNight> list = new List<HardheadNight>();
            if (!req.QueryString.HasValue)
            {
                list = await _hardheadInteractor.GetNextHardheadAsync();
            }

            if (req.Query.ContainsKey("dateFrom") && DateTime.TryParse(req.Query["dateFrom"], out DateTime dateFrom))
            {
                list = await _hardheadInteractor.GetHardheadsAsync(dateFrom);
            }

            if (req.Query.ContainsKey("parentID") && int.TryParse(req.Query["parentID"], out int parentID))
            {
                list = await _hardheadInteractor.GetHardheadsAsync(parentID);
            }

            if (req.Query.ContainsKey("userID") && int.TryParse(req.Query["userID"], out int userID))
            {
                list = await _hardheadInteractor.GetHardheadsAsync(userID, UserType.host);
            }

            if (req.Query.ContainsKey("filter") && req.Query.TryGetValue("filter", out var value))
            {
                list = await _hardheadInteractor.GetHardheadsByMovieAsync(value);
            }

            return new OkObjectResult(list);
        }
    }

    [Function("hardheadActions")]
    public async Task<IActionResult> RunActions([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/{id:int}/actions")] HttpRequest req, int id)
    {
        var method = nameof(RunActions);
        _log.LogInformation("[{Class}.{Function}] C# HTTP trigger function processed a request.", _class, method);

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Class}.{Function}]  JWT is not valid!", _class, method);
            return new UnauthorizedResult();
        }

        try
        {
            var result = await _hardheadInteractor.GetActionsAsync(id, userID);
            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _log.LogError("Internal error", ex);
            throw;
        }
    }

    [Function("hardheadYears")]
    public async Task<IActionResult> GetYears(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/years")] HttpRequest req)
    {
        var method = nameof(GetYears);
        _log.LogInformation("[{Class}.{Function}] C# HTTP trigger function processed a request.", _class, method);

        try
        {
            var years = await _hardheadInteractor.GetYearsAsync();
            return new OkObjectResult(years);
        }
        catch (Exception ex)
        {
            _log.LogError("[{Class}.{Function}] Internal error: {Message}", _class, method, ex.Message);
            throw;
        }
    }

    [Function("hardheadUsers")]
    public async Task<IActionResult> GetUsers(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/{yearId:int}/users")] HttpRequest req,
        int yearId)
    {
        var method = nameof(GetUsers);
        _log.LogInformation("[{Class}.{Function}] C# HTTP trigger function processed a request.", _class, method);

        try
        {
            int minAttendance = 0;
            if (req.Query.ContainsKey("minAttendance") && int.TryParse(req.Query["minAttendance"], out int attendance))
            {
                minAttendance = attendance;
            }

            var users = await _hardheadInteractor.GetUsersByYearAsync(yearId, minAttendance);
            return new OkObjectResult(users);
        }
        catch (Exception ex)
        {
            _log.LogError("[{Class}.{Function}] Internal error: {Message}", _class, method, ex.Message);
            throw;
        }
    }
}
