using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Scripts.UseCases;
using System.Diagnostics;

namespace Ez.Hress.FunctionsApi.News
{
    public class StatisticsNewsFunctions
    {
        private readonly NewsInteractor _newsInteractor;
        public StatisticsNewsFunctions(NewsInteractor newsInteractor)
        {
            _newsInteractor = newsInteractor;
        }

        [FunctionName("statisticsYear")]
        public async Task<IActionResult> RunStatisticsYears(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "news/statistics/years")] HttpRequest req,
            ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunStatisticsYears));

            try
            {
                var list = await _newsInteractor.GetNewsYearStatistics();
                return new OkObjectResult(list);
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[{Function}] Invalid input", nameof(RunStatisticsYears));
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[{Function}] Unhandled error", nameof(RunStatisticsYears));
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation("[{Function}]  Elapsed: {Elapsed} ms.", nameof(RunStatisticsYears), stopwatch.ElapsedMilliseconds);
            }
        }


        [FunctionName("statisticsMonth")]
        public async Task<IActionResult> RunStatisticsMonth(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "news/statistics/years/{year:int}/months")] HttpRequest req, int year,
            ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunStatisticsMonth));

            try
            {
                var list = await _newsInteractor.GetNewsMonthStatistics(year);
                return new OkObjectResult(list);
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[{Function}] Invalid input", nameof(RunStatisticsMonth));
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[{Function}] Unhandled error", nameof(RunStatisticsMonth));
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation("[{Function}]  Elapsed: {Elapsed} ms.", nameof(RunStatisticsMonth), stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
