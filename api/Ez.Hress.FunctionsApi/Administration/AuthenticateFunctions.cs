using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Ez.Hress.FunctionsApi.Administration
{
    public static class AuthenticateFunctions
    {
        [FunctionName("authenticate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            log.LogInformation("[Authenticate] C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AuthenticateBody body = JsonConvert.DeserializeObject<AuthenticateBody>(requestBody);

            log.LogInformation($"[RunAwardNominations] Request username: {body.Username}");

            try
            {
                //await _awardInteractor.Nominate(nom);
                return new NoContentResult();
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[RunAwardNominations] Invalid input");
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[RunAwardNominations] Unhandled error");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation($"[RunAwardNominations] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
            }
        }
    }
}
