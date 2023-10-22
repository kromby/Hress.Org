using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Shared.UseCases;
using Ez.Hress.Hardhead.UseCases;
using Microsoft.VisualBasic;
using System.Collections;
using Ez.Hress.Hardhead.Entities;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;

namespace Ez.Hress.FunctionsApi.Hardhead
{
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
                var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, out int userID, log);
                if (!isJWTValid)
                {
                    log.LogInformation("[{Class}.{Function}]  JWT is not valid!", _class, method);
                    return new UnauthorizedResult();
                }

                try
                {
                    HardheadNight night = await req.ReadFromJsonAsync<HardheadNight>();
                    await _hardheadInteractor.SaveHardhead(night, userID);
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
                    var entity = await _hardheadInteractor.GetHardhead(id.Value);                  
                    return new OkObjectResult(entity);
                }

                IList<HardheadNight> list = new List<HardheadNight>();
                if (!req.QueryString.HasValue)
                {
                    list = await _hardheadInteractor.GetNextHardhead();
                }

                if (req.Query.ContainsKey("dateFrom"))
                {

                    if (DateTime.TryParse(req.Query["dateFrom"], out DateTime dateFrom))
                    {
                        list = await _hardheadInteractor.GetHardheads(dateFrom);
                    }
                }

                if (req.Query.ContainsKey("parentID"))
                {
                    if (int.TryParse(req.Query["parentID"], out int parentID))
                    {
                        list = await _hardheadInteractor.GetHardheads(parentID);
                    }
                }

                if (req.Query.ContainsKey("userID"))
                {
                    if (int.TryParse(req.Query["userID"], out int userID))
                    {
                        list = await _hardheadInteractor.GetHardheads(userID, UserType.host);
                    }
                }

                return new OkObjectResult(list);
            }
        }
    }
}
