using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.FunctionsApi.Beer;

internal static class BeerResults
{
    public static IActionResult From(Exception ex, ILogger log)
    {
        switch (ex)
        {
            case ArgumentException:
                log.LogWarning(ex, "[BeerResults.From] Invalid input");
                return new BadRequestObjectResult(ex.Message);
            case UnauthorizedAccessException:
                log.LogWarning(ex, "[BeerResults.From] Forbidden");
                return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status403Forbidden };
            case KeyNotFoundException:
                log.LogInformation(ex, "[BeerResults.From] Not found");
                return new NotFoundObjectResult(ex.Message);
            case InvalidOperationException:
                log.LogWarning(ex, "[BeerResults.From] Conflict");
                return new ConflictObjectResult(ex.Message);
            default:
                log.LogError(ex, "[BeerResults.From] Unhandled error");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
