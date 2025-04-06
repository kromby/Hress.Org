using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Hardhead.Entities;
using System.Diagnostics;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class HardheadStatisticFunctions
{
    private const string _class = nameof(HardheadStatisticFunctions);

    private readonly HardheadStatisticsInteractor _statisticsInteractor;
    private readonly ILogger<HardheadStatisticFunctions> _log;

    public HardheadStatisticFunctions(HardheadStatisticsInteractor hardheadStatisticsInteractor, ILogger<HardheadStatisticFunctions> log)
    {
        _statisticsInteractor = hardheadStatisticsInteractor;
        _log = log;
    }

    [Function("hardheadStatisticsAttendance")]
    public async Task<IActionResult> RunAttendance(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/statistics/attendances")] HttpRequest req)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        const string method = nameof(RunAttendance);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

        try
        {
            var periodType = PeriodType.All;
            if (req.Query.TryGetValue("periodType", out var periodTypeString))
            {
                _ = Enum.TryParse<PeriodType>(periodTypeString, out periodType);
            }

            var list = await _statisticsInteractor.GetAttendanceStatisticsAsync(periodType);
            return new OkObjectResult(list);

        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _class, method);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _class, method);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Class}.{Method}] Elapsed: {ElapsedMilliseconds} ms.", _class, method, stopwatch.ElapsedMilliseconds);
        }
    }

        [Function("hardheadStatisticUsers")]
    public async Task<IActionResult> RunUsers(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/statistics/users/{id:int?}")] HttpRequest req,
        int? id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        const string method = nameof(RunUsers);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

        try
        {
            var periodType = PeriodType.All;
            if (req.Query.TryGetValue("periodType", out var periodTypeString))
            {
                _ = Enum.TryParse<PeriodType>(periodTypeString, out periodType);
            }

            var attendanceTypeID = 0;
            if (req.Query.TryGetValue("attendanceType", out var attendanceTypeString))
            {
                _ = int.TryParse(attendanceTypeString, out attendanceTypeID);
            }

            var list = await _statisticsInteractor.GetUserStatisticsAsync(periodType, attendanceTypeID, id);
            return new OkObjectResult(list);

        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _class, method);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _class, method);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Class}.{Method}] Elapsed: {ElapsedMilliseconds} ms.", _class, method, stopwatch.ElapsedMilliseconds);
        }
    }

    [Function("hardheadStatisticUserChallenges")]
    public async Task<IActionResult> RunUserChallenges([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/statistics/users/{id:int}/challenges")] HttpRequest req, int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        const string method = nameof(RunUserChallenges);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

        try
        {
            var list = await _statisticsInteractor.GetChallangeHistoryAsync(id);
            return new OkObjectResult(list);

        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _class, method);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _class, method);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Class}.{Method}] Elapsed: {ElapsedMilliseconds} ms.", _class, method, stopwatch.ElapsedMilliseconds);
        }
    }

    [Function("hardheadStatisticUserStreaks")]
    public async Task<IActionResult> RunUserStreaks([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/statistics/users/{id:int}/streaks")] HttpRequest req, int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        const string method = nameof(RunUserStreaks);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

        try
        {
            var list = await _statisticsInteractor.GetStreaksAsync(id);
            return new OkObjectResult(list);

        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _class, method);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _class, method);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Class}.{Method}] Elapsed: {ElapsedMilliseconds} ms.", _class, method, stopwatch.ElapsedMilliseconds);
        }
    }
}
