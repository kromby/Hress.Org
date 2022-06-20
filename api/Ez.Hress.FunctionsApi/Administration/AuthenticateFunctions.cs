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
using Ez.Hress.Shared.UseCases;

namespace Ez.Hress.FunctionsApi.Administration
{
    public class AuthenticateFunctions
    {
        private readonly AuthenticationInteractor _authenticationInteractor;

        public AuthenticateFunctions(AuthenticationInteractor authenticationInteractor)
        {
            _authenticationInteractor = authenticationInteractor;
        }

        [FunctionName("authenticate")]
        public async Task<IActionResult> Run(
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
                var jwt = await _authenticationInteractor.Login(body.Username, body.Password, req.HttpContext.Connection.RemoteIpAddress.ToString());
                return new OkObjectResult(jwt);
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[RunAwardNominations] Invalid input");
                return new BadRequestObjectResult(aex.Message);
            }
            catch(UnauthorizedAccessException uaex)
            {
                log.LogError(uaex, "[RunAwardNominations] Unauthorized");
                return new UnauthorizedResult();
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
