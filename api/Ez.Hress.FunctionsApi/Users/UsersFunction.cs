using Ez.Hress.Shared.UseCases;
using Ez.Hress.UserProfile.UseCases;
using Ez.Hress.UserProfile.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Users;

public class UsersFunction
{
    private readonly string _function = nameof(UsersFunction);
    private readonly UserProfileInteractor _userProfileInteractor;
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly ILogger<UsersFunction> _log;

    public UsersFunction(AuthenticationInteractor authenticationInteractor, UserProfileInteractor userProfileInteractor, ILogger<UsersFunction> log)
    {
        _userProfileInteractor = userProfileInteractor;
        _authenticationInteractor = authenticationInteractor;
        _log = log;
    }

    [Function("users")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id:int}")] HttpRequest req, int id)
    {
        var methodName = nameof(Run);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            var entity = await _userProfileInteractor.GetUserAsync(id);
            return entity == null ? new NotFoundResult() : new OkObjectResult(entity);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _function, methodName);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _function, methodName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[HardheadRuleFunctions] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [Function("usersBalanceSheet")]
    public async Task<IActionResult> RunBalanceSheet(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id:int}/balancesheet")] HttpRequest req,
        int id)
    {
        var methodName = nameof(RunBalanceSheet);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        if (!AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID))
        {
            _log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunBalanceSheet));
            return new UnauthorizedResult();
        }

        var myId = id != 0 ? id : userID;

        bool includePaid = false;
        if(req.Query.ContainsKey("includePaid"))
        {
            bool.TryParse(req.Query["includePaid"], out includePaid);
        }

        var entity = await _userProfileInteractor.GetBalanceSheetAsync(myId, includePaid);
        return new OkObjectResult(entity);
    }

    [Function("userPassword")]
    public async Task<IActionResult> RunPassword([HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/{id:int}/password")] HttpRequest req, int id)
    {
        var methodName = nameof(RunPassword);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        if (!AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID))
        {
            _log.LogInformation("[{Class}.{Method}] JWT is not valid!", _function, methodName);
            return new UnauthorizedResult();
        }

        id = id != 0 ? id : userID;
        if (userID != id)
        {
            _log.LogWarning("[{Class}.{Method}] Don't have access to that user!", _function, methodName);
            return new UnauthorizedObjectResult("Don't have access to that user!");
        }

        try
        {
            var body = await req.ReadFromJsonAsync<ChangePasswordBody>();
            await _authenticationInteractor.ChangePasswordAsync(id, body.Password, body.NewPassword);
            return new AcceptedResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _function, methodName);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (UnauthorizedAccessException uaex)
        {
            _log.LogError(uaex, "[{Class}.{Method}] Unauthorized", _function, methodName);
            return new UnauthorizedResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _function, methodName);
            throw;
        }
    }

    [Function("userLookup")]
    public async Task<IActionResult> RunLookup(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete", Route = "users/{userId:int}/lookups/{id:int?}")] HttpRequest req,
        int userId, int? id)
    {
        var methodName = nameof(RunLookup);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        if (!AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int authenticatedUserID))
        {
            _log.LogInformation("[{Class}.{Method}] JWT is not valid!", _function, methodName);
            return new UnauthorizedResult();
        }

        try
        {
            if (HttpMethods.IsGet(req.Method))
            {
                // Check if query parameter for typeId is provided
                if (req.Query.ContainsKey("typeId") && int.TryParse(req.Query["typeId"], out int typeId))
                {
                    var lookup = await _userProfileInteractor.GetLookupByUserAndTypeAsync(userId, typeId);
                    return lookup == null ? new NotFoundResult() : new OkObjectResult(lookup);
                }

                if (!id.HasValue)
                {
                    return new BadRequestObjectResult("Lookup ID is required for GET request, or provide typeId query parameter");
                }

                var lookupById = await _userProfileInteractor.GetLookupAsync(id.Value);
                if (lookupById == null)
                {
                    return new NotFoundResult();
                }

                // Verify the lookup belongs to the userId in the route
                if (lookupById.UserId != userId)
                {
                    _log.LogWarning("[{Class}.{Method}] Lookup {LookupId} belongs to user {LookupUserId}, not {UserId}", 
                        _function, methodName, id.Value, lookupById.UserId, userId);
                    return new NotFoundResult();
                }

                return new OkObjectResult(lookupById);
            }
            else if (HttpMethods.IsPost(req.Method))
            {
                // Users can only create lookups for themselves
                if (userId != authenticatedUserID)
                {
                    _log.LogWarning("[{Class}.{Method}] User {AuthenticatedUserID} attempted to create lookup for user {UserId}", 
                        _function, methodName, authenticatedUserID, userId);
                    return new UnauthorizedObjectResult("You can only create lookups for yourself");
                }

                var lookup = await req.ReadFromJsonAsync<Lookup>();
                if (lookup == null)
                {
                    return new BadRequestObjectResult("Invalid request body");
                }

                lookup.UserId = userId;
                lookup.InsertedBy = authenticatedUserID;

                var lookupId = await _userProfileInteractor.CreateLookupAsync(lookup);
                return new CreatedResult($"/api/users/{userId}/lookups/{lookupId}", new { id = lookupId });
            }
            else if (HttpMethods.IsPut(req.Method))
            {
                if (!id.HasValue)
                {
                    return new BadRequestObjectResult("Lookup ID is required for PUT request");
                }

                // Users can only update lookups for themselves
                if (userId != authenticatedUserID)
                {
                    _log.LogWarning("[{Class}.{Method}] User {AuthenticatedUserID} attempted to update lookup for user {UserId}", 
                        _function, methodName, authenticatedUserID, userId);
                    return new UnauthorizedObjectResult("You can only update your own lookups");
                }

                var lookup = await req.ReadFromJsonAsync<Lookup>();
                if (lookup == null)
                {
                    return new BadRequestObjectResult("Invalid request body");
                }

                lookup.ID = id.Value;
                lookup.UserId = userId;
                if (!lookup.UpdatedBy.HasValue)
                {
                    lookup.UpdatedBy = authenticatedUserID;
                }

                var rowsAffected = await _userProfileInteractor.UpdateLookupAsync(lookup);
                return rowsAffected > 0 ? new OkObjectResult(lookup) : new NotFoundResult();
            }
            else if (HttpMethods.IsDelete(req.Method))
            {
                if (!id.HasValue)
                {
                    return new BadRequestObjectResult("Lookup ID is required for DELETE request");
                }

                // Users can only delete lookups for themselves
                if (userId != authenticatedUserID)
                {
                    _log.LogWarning("[{Class}.{Method}] User {AuthenticatedUserID} attempted to delete lookup for user {UserId}", 
                        _function, methodName, authenticatedUserID, userId);
                    return new UnauthorizedObjectResult("You can only delete your own lookups");
                }

                var rowsAffected = await _userProfileInteractor.DeleteLookupAsync(id.Value, authenticatedUserID);
                return rowsAffected > 0 ? new NoContentResult() : new NotFoundResult();
            }

            return new BadRequestResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _function, methodName);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _function, methodName);
            throw;
        }
    }
}
