using System.Text.Json;
using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.FunctionsApi.Beer;

public class BeerRatingFunctions
{
    private const string _class = nameof(BeerRatingFunctions);
    private static readonly JsonSerializerOptions RequestJsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly RatingInteractor _ratingInteractor;
    private readonly ILogger<BeerRatingFunctions> _log;

    public BeerRatingFunctions(AuthenticationInteractor authenticationInteractor, RatingInteractor ratingInteractor, ILogger<BeerRatingFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _ratingInteractor = ratingInteractor;
        _log = log;
    }

    [Function("beerRating")]
    public async Task<IActionResult> RunRatingAsync([HttpTrigger(AuthorizationLevel.Function, "put", Route = "beer/seasons/{seasonID}/beers/{beerID}/rating")] HttpRequest req, string seasonID, string beerID)
    {
        const string method = nameof(RunRatingAsync);

        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Class}.{Method}] JWT is not valid!", _class, method);
            return new UnauthorizedResult();
        }

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<RatingRequest>(requestBody, RequestJsonOptions);
            if (request?.Stars == null)
                return new BadRequestObjectResult("Stars are required.");

            var rating = await _ratingInteractor.SaveRatingAsync(seasonID, beerID, userID, request.Stars.Value);
            return new OkObjectResult(rating);
        }
        catch (JsonException)
        {
            // Also covers fractional stars, e.g. {"stars": 3.5}.
            return new BadRequestObjectResult("Stars must be a whole number.");
        }
        catch (Exception ex)
        {
            return BeerResults.From(ex, _log);
        }
    }

    private sealed class RatingRequest
    {
        public int? Stars { get; set; }
    }
}
