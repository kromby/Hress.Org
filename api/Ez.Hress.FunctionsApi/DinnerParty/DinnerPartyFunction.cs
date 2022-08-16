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
using Ez.Hress.MajorEvents.Entities;
using Ez.Hress.Shared.UseCases;

namespace Ez.Hress.FunctionsApi.DinnerParty
{
    public class DinnerPartyFunction
    {
        private readonly AuthenticationInteractor _authenticationInteractor;
        private readonly DinnerPartyInteractor _dinnerPartyInteractor;

        public DinnerPartyFunction(AuthenticationInteractor authenticationInteractor, DinnerPartyInteractor dinnerPartyInteractor)
        {
            _authenticationInteractor = authenticationInteractor;
            _dinnerPartyInteractor = dinnerPartyInteractor;
        }

        
        [FunctionName("dinnerParties")]
        public async Task<IActionResult> RunDinnerParties(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dinnerparties/{id:int?}")] HttpRequest req,
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
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dinnerparties/{id:int}/courses")] HttpRequest req,
            int id, ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunCourses));

            try
            {
                var result = await _dinnerPartyInteractor.GetCourses(id);
                return new OkObjectResult(result);
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

        [FunctionName("dinnerPartiesCoursesByType")]
        public async Task<IActionResult> RunCoursesByType(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "dinnerparties/courses/{typeID:int}")] HttpRequest req,
            int typeID, ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunCoursesByType));

            try
            {
                if (req.Method == "GET")
                {
                    log.LogInformation("[{Function}] Getting a all courses.", nameof(RunCoursesByType));
                    var courses = await _dinnerPartyInteractor.GetCoursesByType(typeID);
                    return new OkObjectResult(courses);
                }
                else
                {
                    var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, out int userID, log);
                    if (!isJWTValid)
                    {
                        log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunCoursesByType));
                        return new UnauthorizedResult();
                    }

                    log.LogInformation("[{Function}] Saving a vote.", nameof(RunCoursesByType));
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var vote = JsonConvert.DeserializeObject<Vote>(requestBody);
                    vote.TypeID = typeID;
                    vote.InsertedBy = userID;

                    log.LogInformation("[{Function}] Request body: {requestBody}", nameof(RunCoursesByType), requestBody);

                    await _dinnerPartyInteractor.SaveVote(vote);
                    return new OkResult();
                }                
            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[{Function}] Invalid input", nameof(RunCoursesByType));
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[{Function}] Unhandled error", nameof(RunCoursesByType));
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation("[{Function}] Elapsed: {Elapsed} ms.", nameof(RunCoursesByType), stopwatch.ElapsedMilliseconds);
            }
        }
    }    
}
