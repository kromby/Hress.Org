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

            if (HttpMethods.IsPut(req.Method)) {
                var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, out int userID, log);
                if (!isJWTValid)
                {
                    log.LogInformation("[{Class}.{Function}]  JWT is not valid!", _class, method);
                    return new UnauthorizedResult();
                }
            } else // GET
            {
                if(id.HasValue)
                {
                    var entity = await _hardheadInteractor.GetHardhead(id.Value);
                    if (entity == null)
                        return new NotFoundResult();

                    return new OkObjectResult(entity);
                }

                IList<HardheadNight> list = new List<HardheadNight>();
                if(!req.QueryString.HasValue)
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

                if(req.Query.ContainsKey("parentID"))
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

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
