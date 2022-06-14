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
using Ez.Hress.Hardhead.DataAccess;
using System.Configuration;
using Azure.Data.Tables;
using Ez.Hress.Shared.UseCases;

namespace Ez.Hress.FunctionsApi.Hardhead
{
    public class HardheadAwards
    {
        private readonly AwardInteractor _awardInteractor;
        private readonly AuthenticationInteractor _authenticationInteractor;

        public HardheadAwards(AuthenticationInteractor authenticationInteractor, AwardInteractor awardInteractor)
        {
            _authenticationInteractor = authenticationInteractor;
            _awardInteractor = awardInteractor;            
        }

        [FunctionName("hardheadAwardsNominations")]
        public async Task<IActionResult> RunAwardNominations(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "hardhead/awards/nominations")] HttpRequest req,
        ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.LogInformation("[RunAwardNominations] C# HTTP trigger function processed a request.");
            log.LogInformation($"[RunAwardNominations] Host: {req.Host.Value}");

            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, out int userID, log);
            if (!isJWTValid)
                return new UnauthorizedResult();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Nomination nom = JsonConvert.DeserializeObject<Nomination>(requestBody);
            nom.CreatedBy = userID;

            log.LogInformation($"[RunAwardNominations] Request body: {requestBody}");

            try
            {
                await _awardInteractor.Nominate(nom);
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
