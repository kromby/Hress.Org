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
    public class NewsFunctions
    {
        private readonly NewsInteractor _newsInteractor;

        public NewsFunctions(NewsInteractor newsInteractor)
        {
            _newsInteractor = newsInteractor;
        }
        
        [FunctionName("news")]
        public async Task<IActionResult> RunNews(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            log.LogInformation("[RunNews] C# HTTP trigger function processed a request.");

            try
            {
                var list = await _newsInteractor.GetLatestNews();
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
