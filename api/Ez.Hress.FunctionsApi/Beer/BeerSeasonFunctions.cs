using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.FunctionsApi.Beer;

public class BeerSeasonFunctions
{
    private const string _class = nameof(BeerSeasonFunctions);

    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly SeasonInteractor _seasonInteractor;
    private readonly ILogger<BeerSeasonFunctions> _log;

    public BeerSeasonFunctions(AuthenticationInteractor authenticationInteractor, SeasonInteractor seasonInteractor, ILogger<BeerSeasonFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _seasonInteractor = seasonInteractor;
        _log = log;
    }

    [Function("beerCurrentSeason")]
    public async Task<IActionResult> RunCurrentSeasonAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "beer/seasons/current")] HttpRequest req)
    {
        const string method = nameof(RunCurrentSeasonAsync);

        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int _);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Class}.{Method}] JWT is not valid!", _class, method);
            return new UnauthorizedResult();
        }

        try
        {
            var season = await _seasonInteractor.GetCurrentSeasonAsync();
            return season == null ? new NotFoundResult() : new OkObjectResult(season);
        }
        catch (Exception ex)
        {
            return BeerResults.From(ex, _log);
        }
    }
}
