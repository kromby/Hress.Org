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
        public async Task<IActionResult> RunAuthenticate(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            log.LogInformation("[RunAuthenticate] C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AuthenticateBody body = JsonConvert.DeserializeObject<AuthenticateBody>(requestBody);

            log.LogInformation($"[RunAuthenticate] Request username: {body.Username}");
            log.LogInformation($"[RunAuthenticate] Request IP Address: {req.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}");

            try
            {
                var jwt = await _authenticationInteractor.Login(body.Username, body.Password, req.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString());
                return new OkObjectResult(jwt);
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[RunAuthenticate] Invalid input");
                return new BadRequestObjectResult(aex.Message);
            }
            catch(UnauthorizedAccessException uaex)
            {
                log.LogError(uaex, "[RunAuthenticate] Unauthorized");
                return new UnauthorizedResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[RunAuthenticate] Unhandled error");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation($"[RunAuthenticate] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
            }
        }
    }
}
