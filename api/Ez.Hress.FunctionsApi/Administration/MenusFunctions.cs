using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ez.Hress.Administration.UseCases;
using Ez.Hress.Shared.UseCases;

namespace Ez.Hress.FunctionsApi.Administration;

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

        if (req.Query.TryGetValue("navigateUrl", out var navigateUrl))
        {
            _ = bool.TryParse(req.Query["fetchChildren"], out bool fetchChildren);
            log.LogInformation($"[RunMenus] navigateUrl: '{navigateUrl}'");

            var itemList = await _menuInteractor.GetMenuItemsAsync(navigateUrl, userID, fetchChildren);
            return new OkObjectResult(itemList);
        }

        var list = await _menuInteractor.GetMenuRootAsync(userID);
        return new OkObjectResult(list);
    }
}
