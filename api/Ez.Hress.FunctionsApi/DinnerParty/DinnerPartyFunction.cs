using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.MajorEvents.UseCases;
using System.Diagnostics;
using Ez.Hress.MajorEvents.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Azure.Functions.Worker;
using System.Net.NetworkInformation;

namespace Ez.Hress.FunctionsApi.DinnerParty;

public class DinnerPartyFunction
{
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly DinnerPartyInteractor _dinnerPartyInteractor;
    private readonly ILogger<DinnerPartyFunction> _log;

    public DinnerPartyFunction(AuthenticationInteractor authenticationInteractor, DinnerPartyInteractor dinnerPartyInteractor, ILogger<DinnerPartyFunction> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _dinnerPartyInteractor = dinnerPartyInteractor;
        _log = log;
    }


    [Function("dinnerParties")]
    public async Task<IActionResult> RunDinnerParties(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dinnerparties/{id:int?}")] HttpRequest req,
        int? id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunDinnerParties));

        try
        {
            if (id.HasValue)
            {
                _log.LogInformation("[{Function}] Getting a specific dinner party.", nameof(RunDinnerParties));
                var dinnerParty = await _dinnerPartyInteractor.GetDinnerPartyAsync(id.Value);
                return new OkObjectResult(dinnerParty);
            }

            _log.LogInformation("[{Function}] Getting a all dinner parties.", nameof(RunDinnerParties));
            _ = int.TryParse(req.Query["top"], out int top);
            _ = bool.TryParse(req.Query["includeGuests"], out bool includeGuests);
            var dinnerParties = await _dinnerPartyInteractor.GetDinnerPartiesAsync(includeGuests, top);
            return new OkObjectResult(dinnerParties);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[RunNews] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[RunNews] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[RunNews] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [Function("dinnerPartiesCourses")]
    public async Task<IActionResult> RunCourses(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dinnerparties/{id:int}/courses")] HttpRequest req,
        int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunCourses));

        try
        {
            var result = await _dinnerPartyInteractor.GetCoursesAsync(id);
            return new OkObjectResult(result);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Function}] Invalid input", nameof(RunCourses));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Function}] Unhandled error", nameof(RunCourses));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Function}] Elapsed: {Elapsed} ms.", nameof(RunCourses), stopwatch.ElapsedMilliseconds);
        }
    }

    [Function("dinnerPartiesCoursesByType")]
    public async Task<IActionResult> RunCoursesByType(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "dinnerparties/courses/{typeID:int}")] HttpRequest req,
        int typeID)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunCoursesByType));

        try
        {
            if (req.Method == "GET")
            {
                _log.LogInformation("[{Function}] Getting a all courses.", nameof(RunCoursesByType));
                var courses = await _dinnerPartyInteractor.GetCoursesByTypeAsync(typeID);
                return new OkObjectResult(courses);
            }

            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
            if (!isJWTValid)
            {
                _log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunCoursesByType));
                return new UnauthorizedResult();
            }

            _log.LogInformation("[{Function}] Saving a vote.", nameof(RunCoursesByType));
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var vote = JsonConvert.DeserializeObject<Vote>(requestBody);
            vote.TypeID = typeID;
            vote.InsertedBy = userID;

            _log.LogInformation("[{Function}] Request body: {requestBody}", nameof(RunCoursesByType), requestBody);

            await _dinnerPartyInteractor.SaveVoteAsync(vote);
            return new OkResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Function}] Invalid input", nameof(RunCoursesByType));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Function}] Unhandled error", nameof(RunCoursesByType));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Function}] Elapsed: {Elapsed} ms.", nameof(RunCoursesByType), stopwatch.ElapsedMilliseconds);
        }
    }

    [Function("dinnerPartiesTeams")]
    public async Task<IActionResult> RunTeams([HttpTrigger(AuthorizationLevel.Function, "get", Route = "dinnerparties/{id:int}/teams")] HttpRequest req,
        int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunTeams));

        try
        {
            var result = await _dinnerPartyInteractor.GetRedwineTeamsAsync(id);
            return new OkObjectResult(result);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Function}] Invalid input", nameof(RunTeams));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Function}] Unhandled error", nameof(RunTeams));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Function}] Elapsed: {Elapsed} ms.", nameof(RunTeams), stopwatch.ElapsedMilliseconds);
        }
    }

    [Function("dinnerPartiesStatistics")]
    public async Task<IActionResult> RunWinners([HttpTrigger(AuthorizationLevel.Function, "get", Route = "dinnerparties/statistic")] HttpRequest req)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunWinners));

        try
        {
            switch (req.Query["type"])
            {
                case "winners":
                    var result = await _dinnerPartyInteractor.GetWinnerStatisticsAsync();
                    return new OkObjectResult(result);
                case "guests":
                    var guestResult = await _dinnerPartyInteractor.GetGuestStatisticAsync();
                    return new OkObjectResult(guestResult);
                default:
                    throw new ArgumentException("Query parameter type is missing or invalid.");
            }
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Function}] Invalid input", nameof(RunWinners));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Function}] Unhandled error", nameof(RunWinners));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Function}] Elapsed: {Elapsed} ms.", nameof(RunWinners), stopwatch.ElapsedMilliseconds);
        }
    }
}
