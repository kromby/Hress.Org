using Ez.Hress.Scripts.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.News;

public class StatisticsNewsFunctions
{
    private readonly NewsInteractor _newsInteractor;
    private readonly ILogger<StatisticsNewsFunctions> _log;

    public StatisticsNewsFunctions(NewsInteractor newsInteractor, ILogger<StatisticsNewsFunctions> log)
    {
        _newsInteractor = newsInteractor;
        _log = log;
    }

    [Function("statisticsYear")]
    public async Task<IActionResult> RunStatisticsYears(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "news/statistics/years")] HttpRequest req)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        var methodName = nameof(RunStatisticsYears);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(StatisticsNewsFunctions), methodName);

        try
        {
            var list = await _newsInteractor.GetNewsYearStatisticsAsync();
            return new OkObjectResult(list);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Function}] Invalid input", nameof(RunStatisticsYears));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Function}] Unhandled error", nameof(RunStatisticsYears));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Function}]  Elapsed: {Elapsed} ms.", nameof(RunStatisticsYears), stopwatch.ElapsedMilliseconds);
        }
    }


    [Function("statisticsMonth")]
    public async Task<IActionResult> RunStatisticsMonth(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "news/statistics/years/{year:int}/months")] HttpRequest req, int year)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var methodName = nameof(RunStatisticsMonth);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(StatisticsNewsFunctions), methodName);

        try
        {
            var list = await _newsInteractor.GetNewsMonthStatisticsAsync(year);
            return new OkObjectResult(list);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Function}] Invalid input", nameof(RunStatisticsMonth));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Function}] Unhandled error", nameof(RunStatisticsMonth));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Function}]  Elapsed: {Elapsed} ms.", nameof(RunStatisticsMonth), stopwatch.ElapsedMilliseconds);
        }
    }
}
