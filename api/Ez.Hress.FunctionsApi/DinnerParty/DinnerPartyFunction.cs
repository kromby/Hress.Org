using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.MajorEvents.UseCases;
using System.Diagnostics;

namespace Ez.Hress.FunctionsApi.DinnerParty
{
    public class DinnerPartyFunction
    {
        private DinnerPartyInteractor _dinnerPartyInteractor;

        public DinnerPartyFunction(DinnerPartyInteractor dinnerPartyInteractor)
        {
            _dinnerPartyInteractor = dinnerPartyInteractor;
        }

        
        [FunctionName("dinnerParties")]
        public async Task<IActionResult> RunDinnerParties(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "dinnerparties/{id:int?}")] HttpRequest req,
            int? id, ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunDinnerParties));

            try
            {
                if (id.HasValue)
                {
                    log.LogInformation("[{Function}] Getting a specific dinner party.", nameof(RunDinnerParties));
                    var dinnerParty = await _dinnerPartyInteractor.GetDinnerParty(id.Value);
                    return new OkObjectResult(dinnerParty);
                }
                else
                {
                    log.LogInformation("[{Function}] Getting a all dinner parties.", nameof(RunDinnerParties));
                    var dinnerParties = await _dinnerPartyInteractor.GetDinnerParties();
                    return new OkObjectResult(dinnerParties);
                }
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

        [FunctionName("dinnerPartiesCourses")]
        public async Task<IActionResult> RunCourses(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "dinnerparties/courses/{typeID:int}")] HttpRequest req,
            int typeID, ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunCourses));

            try
            {
                var list = await _dinnerPartyInteractor.GetCoursesByType(typeID);
                return new OkObjectResult(list);
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[{Function}] Invalid input", nameof(RunCourses));
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[{Function}] Unhandled error", nameof(RunCourses));
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation("[{Function}] Elapsed: {Elapsed} ms.", nameof(RunCourses), stopwatch.ElapsedMilliseconds);
            }
        }
    }    
}
