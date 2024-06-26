using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Shared.UseCases;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Hardhead.Entities;
using System.Collections.Generic;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class HardheadFunctions
{
    private readonly string _class = nameof(HardheadFunctions);
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly HardheadInteractor _hardheadInteractor;
    public HardheadFunctions(AuthenticationInteractor authenticationInteractor, HardheadInteractor hardheadInteractor)
    {
        _authenticationInteractor = authenticationInteractor;
        _hardheadInteractor = hardheadInteractor;
    }

    [FunctionName("hardhead")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "put", Route = "hardhead/{id:int?}")] HttpRequest req, int? id,
        ILogger log)
    {
        var method = nameof(Run);
        log.LogInformation("[{Class}.{Function}] C# HTTP trigger function processed a request.", _class, method);

        if (HttpMethods.IsPut(req.Method))
        {
            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);
            if (!isJWTValid)
            {
                log.LogInformation("[{Class}.{Function}]  JWT is not valid!", _class, method);
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
                log.LogWarning("[{Class}.{Function}] PUT - Can't parse request body: {body}, exception: {exception}", _class, method, await req.ReadAsStringAsync(), jrex.Message);
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

            return new OkObjectResult(list);
        }
    }

    [FunctionName("hardheadActions")]
    public async Task<IActionResult> RunActions([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/{id:int}/actions")] HttpRequest req, int id, ILogger log)
    {
        var method = nameof(RunActions);
        log.LogInformation("[{Class}.{Function}] C# HTTP trigger function processed a request.", _class, method);

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);
        if (!isJWTValid)
        {
            log.LogInformation("[{Class}.{Function}]  JWT is not valid!", _class, method);
            return new UnauthorizedResult();
        }

        try
        {
            var result = await _hardheadInteractor.GetActionsAsync(id, userID);
            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            log.LogError("Internal error", ex);
            throw;
        }
    }
}
