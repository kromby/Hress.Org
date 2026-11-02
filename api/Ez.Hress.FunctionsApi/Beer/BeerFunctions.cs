using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.FunctionsApi.Beer;

public class BeerFunctions
{
    private const string _class = nameof(BeerFunctions);

    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly SummaryInteractor _summaryInteractor;
    private readonly ILogger<BeerFunctions> _log;

    public BeerFunctions(AuthenticationInteractor authenticationInteractor, SummaryInteractor summaryInteractor, ILogger<BeerFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _summaryInteractor = summaryInteractor;
        _log = log;
    }

    [Function("beerSeasonBeers")]
    public async Task<IActionResult> RunSeasonBeersAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "beer/seasons/{seasonID}/beers")] HttpRequest req, string seasonID)
    {
        const string method = nameof(RunSeasonBeersAsync);

        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Class}.{Method}] JWT is not valid!", _class, method);
            return new UnauthorizedResult();
        }

        try
        {
            var summaries = await _summaryInteractor.GetSeasonSummariesAsync(seasonID, userID);
            return new OkObjectResult(summaries);
        }
        catch (Exception ex)
        {
            return BeerResults.From(ex, _log);
        }
    }
}
