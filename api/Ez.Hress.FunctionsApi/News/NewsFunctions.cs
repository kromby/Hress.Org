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
using System.Collections.Generic;

namespace Ez.Hress.FunctionsApi.News
{
    public class NewsFunctions
    {
        private readonly NewsInteractor _newsInteractor;

        public NewsFunctions(NewsInteractor newsInteractor)
        {
            _newsInteractor = newsInteractor;
        }

        [FunctionName("news")]
        public async Task<IActionResult> RunNews(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "news/{id:int?}")] HttpRequest req, int? id,
            ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunNews));

            try
            {
                IList<Scripts.Entities.News> list = null;
                if (id.HasValue)
                {
                    log.LogInformation("[{Function}] Getting a single news by ID '{Id}'", nameof(RunNews), id);
                    var entity = await _newsInteractor.GetNewsAsync(id.Value);
                    if (entity == null || entity.ID < 1)
                        return new NotFoundResult();
                    return new OkObjectResult(entity);
                }
                
                if (!string.IsNullOrWhiteSpace(req.Query["type"]) && req.Query["type"].ToString().ToUpper().Equals("ONTHISDAY"))
                {
                    log.LogInformation("[{Function}] Getting news for type '{Type}'", nameof(RunNews), req.Query["type"]);
                    int top = int.MaxValue;
                    _ = int.TryParse(req.Query["top"], out top);

                    list = await _newsInteractor.GetNewsOnThisDayAsync(top);
                    return new OkObjectResult(list);
                }
                
                if (!string.IsNullOrWhiteSpace(req.Query["year"]))
                {
                    if (!int.TryParse(req.Query["year"], out int year))
                    {
                        return new BadRequestObjectResult("Year property invalid");
                    }
                    
                    if (int.TryParse(req.Query["month"], out int month))
                    {
                        log.LogInformation("[{Function}] Getting news for year '{Year}' and month '{Month}'", nameof(RunNews), year, month);
                        list = await _newsInteractor.GetNewsByYearAndMonthAsync(year, month);
                    }
                    else
                    {
                        log.LogInformation("[{Function}] Getting news by Year '{Year}'", nameof(RunNews), year);
                        list = await _newsInteractor.GetNewsByYearAsync(year);
                    }
                    return new OkObjectResult(list);
                }

                log.LogInformation("[{Function}] Getting latest news", nameof(RunNews));
                list = await _newsInteractor.GetLatestNewsAsync();
                return new OkObjectResult(list);
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[RunNews] Invalid input");
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[RunNews] Unhandled error");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation($"[RunNews] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
            }
        }
    }
}
