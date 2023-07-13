using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Shared.UseCases;
using Ez.Hress.Hardhead.UseCases;
using System.Diagnostics;
using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.FunctionsApi.Hardhead
{
    public class HardheadRuleFunctions
    {
        private readonly AuthenticationInteractor _authenticationInteractor;
        private readonly RuleChangeInteractor _ruleChangeInteractor;

        public HardheadRuleFunctions(AuthenticationInteractor authenticationInteractor, RuleChangeInteractor ruleChangeInteractor)
        {
            _ruleChangeInteractor = ruleChangeInteractor;
            _authenticationInteractor = authenticationInteractor;
        }

        [FunctionName("hardheadRuleFunctions")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "hardhead/rules/changes")] HttpRequest req,
            ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.LogInformation("[HardheadRuleFunctions] C# HTTP trigger function processed a request.");
            log.LogInformation($"[HardheadRuleFunctions] Host: {req.Host.Value}");

            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, out int userID, log);
            if (!isJWTValid)
            {
                log.LogInformation($"[HardheadRuleFunctions] JWT is not valid!");
                return new UnauthorizedResult();
            }

            try
            {
                if(HttpMethods.IsPost(req.Method))
                {
                    return await PostRuleChange(req, userID, log);
                }
                else
                {
                    log.LogError($"[HardheadRuleFunctions] HttpMethod '{req.Method}' ist not yet supported.");
                    return new NotFoundResult();
                }
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[HardheadRuleFunctions] Invalid input");
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[HardheadRuleFunctions] Unhandled error");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation($"[HardheadRuleFunctions] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        private async Task<IActionResult> PostRuleChange(HttpRequest req, int userID, ILogger log)
        {
            log.LogInformation("[HardheadRuleFunctions] PostAwardNominations");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            RuleChange change = JsonConvert.DeserializeObject<RuleChange>(requestBody);
            change.InsertedBy = userID;

            log.LogInformation($"[HardheadRuleFunctions] Request body: {requestBody}");

            await _ruleChangeInteractor.SubmitRuleChange(change);
            return new NoContentResult();
        }
    }
}
