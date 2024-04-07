using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Administration.UseCases;
using Ez.Hress.Shared.UseCases;

namespace Ez.Hress.FunctionsApi.Administration
{
    public class MenusFunctions
    {
        private readonly MenuInteractor _menuInteractor;
        private readonly AuthenticationInteractor _authenticationInteractor;

        public MenusFunctions(AuthenticationInteractor authenticationInteractor, MenuInteractor menuInteractor)
        {
            _authenticationInteractor = authenticationInteractor;
            _menuInteractor = menuInteractor;
        }
        
        [FunctionName("menus")]
        public async Task<IActionResult> RunMenus(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("[RunMenus] C# HTTP trigger function processed a request.");

            _ = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);

            if (req.Query.ContainsKey("navigateUrl"))
            {
                var navigateUrl = req.Query["navigateUrl"];
                _ = bool.TryParse(req.Query["fetchChildren"], out bool fetchChildren);
                log.LogInformation($"[RunMenus] navigateUrl: '{navigateUrl}'");

                var list = await _menuInteractor.GetMenuItems(navigateUrl, userID, fetchChildren);
                return new OkObjectResult(list);
            }
            else
            {
                var list = await _menuInteractor.GetMenuRoot(userID);
                return new OkObjectResult(list);
            }
        }
    }
}
