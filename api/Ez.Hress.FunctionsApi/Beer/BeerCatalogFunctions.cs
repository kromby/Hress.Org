using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.FunctionsApi.Beer;

public class BeerCatalogFunctions
{
    private const string _class = nameof(BeerCatalogFunctions);

    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly BeerCatalogInteractor _beerCatalogInteractor;
    private readonly ILogger<BeerCatalogFunctions> _log;

    public BeerCatalogFunctions(AuthenticationInteractor authenticationInteractor, BeerCatalogInteractor beerCatalogInteractor, ILogger<BeerCatalogFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _beerCatalogInteractor = beerCatalogInteractor;
        _log = log;
    }

    [Function("beerCatalog")]
    public async Task<IActionResult> RunSearchAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "beer/beers")] HttpRequest req)
    {
        const string method = nameof(RunSearchAsync);

        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Class}.{Method}] JWT is not valid!", _class, method);
            return new UnauthorizedResult();
        }

        try
        {
            string? query = req.Query["query"];
            var beers = await _beerCatalogInteractor.SearchAsync(query);
            return new OkObjectResult(beers);
        }
        catch (Exception ex)
        {
            return BeerResults.From(ex, _log);
        }
    }
}
