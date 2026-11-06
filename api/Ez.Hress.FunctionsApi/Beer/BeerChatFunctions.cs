using System.Text.Json;
using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.FunctionsApi.Beer;

public class BeerChatFunctions
{
    private const string _class = nameof(BeerChatFunctions);
    private static readonly JsonSerializerOptions RequestJsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly BeerChatInteractor _chatInteractor;
    private readonly ILogger<BeerChatFunctions> _log;

    public BeerChatFunctions(AuthenticationInteractor authenticationInteractor, BeerChatInteractor chatInteractor, ILogger<BeerChatFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _chatInteractor = chatInteractor;
        _log = log;
    }

    [Function("beerMessages")]
    public async Task<IActionResult> RunMessagesAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "beer/seasons/{seasonID}/beers/{beerID}/messages")] HttpRequest req, string seasonID, string beerID)
    {
        const string method = nameof(RunMessagesAsync);

        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Class}.{Method}] JWT is not valid!", _class, method);
            return new UnauthorizedResult();
        }

        try
        {
            if (HttpMethods.IsPost(req.Method))
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<MessageRequest>(requestBody, RequestJsonOptions);

                var message = await _chatInteractor.PostMessageAsync(seasonID, beerID, userID, request?.Text);
                return new ObjectResult(message) { StatusCode = StatusCodes.Status201Created };
            }

            var messages = await _chatInteractor.GetMessagesAsync(seasonID, beerID);
            return new OkObjectResult(messages);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Invalid message.");
        }
        catch (Exception ex)
        {
            return BeerResults.From(ex, _log);
        }
    }

    private sealed class MessageRequest
    {
        public string? Text { get; set; }
    }
}
