using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Ez.Hress.Shared.UseCases;
using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class HardheadRatingFunctions
{
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly HardheadInteractor _hardheadInteractor;
    private readonly string _class = nameof(HardheadRatingFunctions);

    public HardheadRatingFunctions(AuthenticationInteractor authenticationInteractor, HardheadInteractor hardheadInteractor)
    {
        _authenticationInteractor = authenticationInteractor;
        _hardheadInteractor = hardheadInteractor;
    }

    [FunctionName("ratings")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "hardhead/{id:int}/ratings")] HttpRequest req, int id,
        ILogger log)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, nameof(Run));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);

        try
        {
            if (HttpMethods.IsPost(req.Method))
            {
                if (!isJWTValid)
                {
                    log.LogInformation("[{Class}.{Function}] JWT is not valid!", _class, nameof(Run));
                    return new UnauthorizedResult();
                }

                throw new NotImplementedException();
            }
            else
            {
                RatingEntity rating = null;
                if (userID == -1)
                    rating = await _hardheadInteractor.GetRating(id).ConfigureAwait(false);
                else
                    rating = await _hardheadInteractor.GetRating(id, userID).ConfigureAwait(false);

                if (rating != null)
                    return new OkObjectResult(rating);


                return new NotFoundResult();
            }
        }
        catch (ArgumentException aex)
        {
            log.LogError(aex, "[{Class}.{Method}] Invalid input", _class, nameof(Run));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "[{Class}.{Method}] Unhandled error", _class, nameof(Run));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            log.LogInformation("[{Class}.{Method}] Elapsed: {Elapsed} ms.", _class, nameof(Run), stopwatch.ElapsedMilliseconds);
        }
    }
}
