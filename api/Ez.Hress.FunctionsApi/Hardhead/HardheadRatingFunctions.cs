using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Ez.Hress.Shared.UseCases;
using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Hardhead;

public class HardheadRatingFunctions
{
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly HardheadInteractor _hardheadInteractor;
    private readonly ILogger<HardheadRatingFunctions> _log;
    private readonly string _class = nameof(HardheadRatingFunctions);

    public HardheadRatingFunctions(AuthenticationInteractor authenticationInteractor, HardheadInteractor hardheadInteractor, ILogger<HardheadRatingFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _hardheadInteractor = hardheadInteractor;
        _log = log;
    }

    [Function("ratings")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "hardhead/{id:int}/ratings")] HttpRequest req, int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, nameof(Run));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);

        try
        {
            if (HttpMethods.IsPost(req.Method))
            {
                if (!isJWTValid)
                {
                    _log.LogInformation("[{Class}.{Function}] JWT is not valid!", _class, nameof(Run));
                    return new UnauthorizedResult();
                }

                throw new NotImplementedException();
            }
            else
            {
                if (id <= 0)
                {
                    _log.LogInformation("[{Class}.{Function}] Invalid ID: {ID}", _class, nameof(Run), id);
                    return new BadRequestObjectResult("Invalid ID");
                }

                RatingEntity rating = await _hardheadInteractor.GetRatingAsync(id, userID == -1 ? null : userID).ConfigureAwait(false);

                if (rating != null)
                    return new OkObjectResult(rating);


                return new NotFoundResult();
            }
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _class, nameof(Run));
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _class, nameof(Run));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Class}.{Method}] Elapsed: {Elapsed} ms.", _class, nameof(Run), stopwatch.ElapsedMilliseconds);
        }
    }
}
