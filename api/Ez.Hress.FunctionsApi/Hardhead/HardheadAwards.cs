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
using Ez.Hress.Hardhead.Entities;
using System.Diagnostics;

namespace Ez.Hress.FunctionsApi.Hardhead
{
    public class HardheadAwards
    {
        private readonly AwardInteractor _awardInteractor;

        public HardheadAwards(AwardInteractor awardInteractor)
        {
            _awardInteractor = awardInteractor;
        }

        [FunctionName("hardheadAwards")]
        public async Task<IActionResult> RunAwards(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/awards")] HttpRequest req,
            ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            log.LogInformation("C# HTTP trigger RunAwards function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //var json = JsonConvert.DeserializeObject(requestBody);
            Nomination nom = JsonConvert.DeserializeObject<Nomination>(requestBody);

            log.LogInformation($"Request body: {requestBody}");

            stopwatch.Stop();
            log.LogInformation($"Elapsed: {stopwatch.ElapsedMilliseconds} ms.");

            return new OkResult();
        }

            [FunctionName("hardheadAwardsNominations")]
        public async Task<IActionResult> RunAwardNominations(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/awards/nominations")] HttpRequest req,
            ILogger log)
        {

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.LogInformation("C# HTTP trigger RunAwardNominations function processed a request.");

            // string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //var json = JsonConvert.DeserializeObject(requestBody);
            Nomination nom = JsonConvert.DeserializeObject<Nomination>(requestBody);

            log.LogInformation($"Request body: {requestBody}");

            try
            {
                _awardInteractor.Nominate(nom);
                log.LogInformation("Return OK - No Content");
                return new NoContentResult();
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "Invalid input");
                return new BadRequestObjectResult(aex.Message);
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Unhandled error");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation($"Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
            }
        }
    }
}
