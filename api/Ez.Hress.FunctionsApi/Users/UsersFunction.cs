using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.UserProfile.UseCases;
using Ez.Hress.Shared.UseCases;
using Ez.Hress.FunctionsApi.Administration;
using System.Diagnostics;

namespace Ez.Hress.FunctionsApi.Users
{
    public class UsersFunction
    {
        private readonly string _function = nameof(UsersFunction);
        private readonly UserProfileInteractor _userProfileInteractor;
        private readonly AuthenticationInteractor _authenticationInteractor;

        public UsersFunction(AuthenticationInteractor authenticationInteractor, UserProfileInteractor userProfileInteractor)
        {
            _userProfileInteractor = userProfileInteractor;
            _authenticationInteractor = authenticationInteractor;
        }

        [FunctionName("users")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id:int}")] HttpRequest req, int id,
            ILogger log)
        {
            log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, nameof(Run));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var entity = await _userProfileInteractor.GetUser(id);
                return entity == null ? new NotFoundResult() : new OkObjectResult(entity);
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

        [FunctionName("usersBalanceSheet")]
        public async Task<IActionResult> RunBalanceSheet(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id:int}/balancesheet")] HttpRequest req,
            int id, ILogger log)
        {
            log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, nameof(RunBalanceSheet));

            if (!AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID))
            {
                log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunBalanceSheet));
                return new UnauthorizedResult();
            }

            var myId = id != 0 ? id : userID;
            var entity = await _userProfileInteractor.GetBalanceSheet(myId);
            return new OkObjectResult(entity);
        }

        [FunctionName("userPassword")]
        public async Task<IActionResult> RunPassword([HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/{id:int}/password")] HttpRequest req, int id, ILogger log)
        {
            log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, nameof(RunPassword));

            if (!AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID))
            {
                log.LogInformation("[{Class}.{Method}] JWT is not valid!", _function, nameof(RunPassword));
                return new UnauthorizedResult();
            }

            id = id != 0 ? id : userID;
            if (userID != id)
            {
                log.LogWarning("[{Class}.{Method}] Don't have access to that user!", _function, nameof(RunPassword));
                return new UnauthorizedObjectResult("Don't have access to that user!");
            }

            try
            {
                var body = await req.ReadFromJsonAsync<ChangePasswordBody>();
                await _authenticationInteractor.ChangePassword(id, body.Password, body.NewPassword);
                return new AcceptedResult();
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[{Class}.{Method}] Invalid input", _function, nameof(RunPassword));
                return new BadRequestObjectResult(aex.Message);
            }
            catch (UnauthorizedAccessException uaex)
            {
                log.LogError(uaex, "[{Class}.{Method}] Unauthorized", _function, nameof(RunPassword));
                return new UnauthorizedResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[{Class}.{Method}] Unhandled error", _function, nameof(RunPassword));
                throw;
            }
        }
    }
}
