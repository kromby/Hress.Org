using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Hardhead.UseCases;
using System.Diagnostics;
using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.FunctionsApi.Hardhead
{
    public class MovieFunctions
    {
        private readonly string _class = nameof(HardheadFunctions);
        //private readonly AuthenticationInteractor _authenticationInteractor;
        private readonly MovieInteractor _movieInteractor;

        public MovieFunctions(MovieInteractor movieInteractor)
        {
            _movieInteractor = movieInteractor;
        }

        [FunctionName("MovieFunctions")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "movies/{id:int?}")] HttpRequest req, int? id,
            ILogger log)
        {
            log.LogInformation("[{Class}]C# HTTP trigger function processed a request.", _class);

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

        [FunctionName("movieStatistics")]
        public async Task<IActionResult> RunStatistics([HttpTrigger(AuthorizationLevel.Function, "get", Route = "movies/statistics/{type}")] HttpRequest req, string type, ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunStatistics));

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
                log.LogError(aex, "[{Function}] Invalid input", nameof(RunStatistics));
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[{Function}] Unhandled error", nameof(RunStatistics));
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation("[{Function}] Elapsed: {Elapsed} ms.", nameof(RunStatistics), stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
