using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.Entities.InputModels;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class MovieFunctions
{
    private readonly string _class = nameof(HardheadFunctions);
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly MovieInteractor _movieInteractor;
    private readonly HardheadInteractor _hardheadInteractor;
    private readonly HardheadParser _hardheadParser;
    private readonly ILogger<MovieFunctions> _log;

    public MovieFunctions(AuthenticationInteractor authenticationInteractor, MovieInteractor movieInteractor, HardheadInteractor hardheadInteractor, HardheadParser hardheadParser, ILogger<MovieFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _movieInteractor = movieInteractor;
        _hardheadInteractor = hardheadInteractor;
        _hardheadParser = hardheadParser;
        _log = log;
    }

    [Function("MovieFunctions")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "movies/{id:int?}")] HttpRequest req, int? id)
    {
        var methodName = nameof(Run);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, methodName);

        if (HttpMethods.IsGet(req.Method))
        {
            if (id.HasValue)
            {

            }
            else
            {
                if (req.Query.TryGetValue("filter", out var value))
                {
                    var list = await _movieInteractor.GetMoviesAsync(value);
                    return new OkObjectResult(list);
                }
            }
        }

        return new NotFoundResult();
    }

    [Function("MovieInformation")]
    public async Task<IActionResult> RunMovieInfoAsync([HttpTrigger(AuthorizationLevel.Function, "post", "put", Route = "movies/{id:int}/info")] HttpRequest req, int id)
    {
        var methodName = nameof(RunMovieInfoAsync);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, methodName);

        if (HttpMethods.IsPost(req.Method) || HttpMethods.IsPut(req.Method))
        {
            try
            {
                var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
                if (!isJWTValid)
                {
                    _log.LogInformation("[{Class}.{Function}]  JWT is not valid!", _class, methodName);
                    return new UnauthorizedResult();
                }

                var movieInfoInput = await req.ReadFromJsonAsync<MovieInfoModel>();
                if (movieInfoInput == null)
                {
                    return new BadRequestObjectResult("Invalid input");
                }

                var movieInfo = _hardheadParser.ParseMovieInfo(movieInfoInput);

                var hardheadNight = await _hardheadInteractor.GetHardheadAsync(id);

                if (hardheadNight == null)
                {
                    return new NotFoundResult();
                }

                await _movieInteractor.SaveMovieInformationAsync(id, userID, hardheadNight.Date, movieInfo);

                return new OkResult();
            }
            catch (SystemException sex)
            {
                _log.LogError(sex, "[{Class}.{Method}] Error saving movie information.", _class, methodName);
                throw;
            }
        }

        return new NotFoundResult();
    }

    [Function("MovieStatistics")]
    public async Task<IActionResult> RunStatistics([HttpTrigger(AuthorizationLevel.Function, "get", Route = "movies/statistics/{type}")] HttpRequest req, string type)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunStatistics));

        try
        {
            var periodTypeString = req.Query["periodType"];
            var periodType = PeriodType.All;
            if (!string.IsNullOrEmpty(periodTypeString))
            {
                _ = Enum.TryParse<PeriodType>(periodTypeString, out periodType);
            }

            if (type == "actor")
            {
                var result = await _movieInteractor.GetActorStatisticsAsync(periodType);
                return new OkObjectResult(result);
            }

            return new NotFoundResult();

        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Function}] Invalid input", nameof(RunStatistics));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Function}] Unhandled error", nameof(RunStatistics));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Function}] Elapsed: {Elapsed} ms.", nameof(RunStatistics), stopwatch.ElapsedMilliseconds);
        }
    }
}
