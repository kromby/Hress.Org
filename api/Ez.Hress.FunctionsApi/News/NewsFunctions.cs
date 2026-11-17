using Ez.Hress.Scripts.Entities;
using Ez.Hress.Scripts.UseCases;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.News;

public class NewsRequest
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Public { get; set; } = true;
    public int? ImageId { get; set; }
    public NewsImageRequest? Image { get; set; }
    public int? ImageAlign { get; set; }

    // Accept both { "imageId": 1 } and { "image": { "id": 1 } }.
    public int? ResolvedImageId => ImageId ?? Image?.Id;

    public Scripts.Entities.News ToNews() => new()
    {
        Name = Name,
        Content = Content,
        Public = Public,
        Image = ResolvedImageId.HasValue ? new ImageHrefEntity(ResolvedImageId.Value, string.Empty) : null,
        ImageAlign = (Align)(ImageAlign ?? 0),
    };
}

public class NewsImageRequest
{
    public int Id { get; set; }
}

public class NewsFunctions
{
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly NewsInteractor _newsInteractor;
    private readonly ILogger<NewsFunctions> _log;

    private static readonly JsonSerializerOptions RequestJsonOptions = new() { PropertyNameCaseInsensitive = true };

    public NewsFunctions(AuthenticationInteractor authenticationInteractor, NewsInteractor newsInteractor, ILogger<NewsFunctions> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _newsInteractor = newsInteractor;
        _log = log;
    }

    [Function("news")]
    public async Task<IActionResult> RunNews(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "news/{id:int?}")] HttpRequest req, int? id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var methodName = nameof(RunNews);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(NewsFunctions), methodName);

        try
        {
            IList<Scripts.Entities.News> list = new List<Scripts.Entities.News>();
            if (id.HasValue)
            {
                _log.LogInformation("[{Function}] Getting a single news by ID '{Id}'", nameof(RunNews), id);
                var entity = await _newsInteractor.GetNewsAsync(id.Value);
                if (entity == null || entity.ID < 1)
                    return new NotFoundResult();
                return new OkObjectResult(entity);
            }
            
            if (!string.IsNullOrWhiteSpace(req.Query["type"]) && req.Query["type"].ToString().ToUpper().Equals("ONTHISDAY"))
            {
                _log.LogInformation("[{Function}] Getting news for type '{Type}'", nameof(RunNews), req.Query["type"]);
                int top = int.MaxValue;
                _ = int.TryParse(req.Query["top"], out top);

                list = await _newsInteractor.GetNewsOnThisDayAsync(top);
                return new OkObjectResult(list);
            }
            
            if (!string.IsNullOrWhiteSpace(req.Query["year"]))
            {
                if (!int.TryParse(req.Query["year"], out int year))
                {
                    return new BadRequestObjectResult("Year property invalid");
                }
                
                if (int.TryParse(req.Query["month"], out int month))
                {
                    _log.LogInformation("[{Function}] Getting news for year '{Year}' and month '{Month}'", nameof(RunNews), year, month);
                    list = await _newsInteractor.GetNewsByYearAndMonthAsync(year, month);
                }
                else
                {
                    _log.LogInformation("[{Function}] Getting news by Year '{Year}'", nameof(RunNews), year);
                    list = await _newsInteractor.GetNewsByYearAsync(year);
                }
                return new OkObjectResult(list);
            }

            _log.LogInformation("[{Function}] Getting latest news", nameof(RunNews));
            list = await _newsInteractor.GetLatestNewsAsync();
            return new OkObjectResult(list);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[RunNews] Invalid input");
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[RunNews] Unhandled error");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[RunNews] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [Function("newsMine")]
    public async Task<IActionResult> RunNewsMine(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "news/mine")] HttpRequest req)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var methodName = nameof(RunNewsMine);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(NewsFunctions), methodName);

        try
        {
            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
            if (!isJWTValid)
            {
                _log.LogInformation("[{Class}.{Method}] JWT is not valid!", nameof(NewsFunctions), methodName);
                return new UnauthorizedResult();
            }

            _log.LogInformation("[{Class}.{Method}] Getting news for author '{UserId}'", nameof(NewsFunctions), methodName, userID);
            var list = await _newsInteractor.GetNewsByAuthorAsync(userID);
            return new OkObjectResult(list);
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", nameof(NewsFunctions), methodName);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", nameof(NewsFunctions), methodName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _log.LogInformation("[{Class}.{Method}] Elapsed: {Elapsed} ms.", nameof(NewsFunctions), methodName, stopwatch.ElapsedMilliseconds);
        }
    }

    [Function("newsPost")]
    public async Task<IActionResult> RunNewsCreateAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "news")] HttpRequest req)
    {
        var methodName = nameof(RunNewsCreateAsync);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(NewsFunctions), methodName);

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Class}.{Method}] JWT is not valid!", nameof(NewsFunctions), methodName);
            return new UnauthorizedResult();
        }

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<NewsRequest>(requestBody, RequestJsonOptions);
            if (request == null)
                return new BadRequestResult();

            var created = await _newsInteractor.CreateNewsAsync(request.ToNews(), userID);
            return new CreatedResult($"/api/news/{created.ID}", created);
        }
        catch (JsonException)
        {
            return new BadRequestResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogWarning("[{Class}.{Method}] Validation failed: {Message}", nameof(NewsFunctions), methodName, aex.Message);
            return new BadRequestObjectResult(new { error = aex.Message });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", nameof(NewsFunctions), methodName);
            throw;
        }
    }

    [Function("newsPut")]
    public async Task<IActionResult> RunNewsUpdateAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "news/{id:int}")] HttpRequest req, int id)
    {
        var methodName = nameof(RunNewsUpdateAsync);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(NewsFunctions), methodName);

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Class}.{Method}] JWT is not valid!", nameof(NewsFunctions), methodName);
            return new UnauthorizedResult();
        }

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<NewsRequest>(requestBody, RequestJsonOptions);
            if (request == null)
                return new BadRequestResult();

            var updated = await _newsInteractor.UpdateNewsAsync(id, request.ToNews(), userID);
            return new OkObjectResult(updated);
        }
        catch (JsonException)
        {
            return new BadRequestResult();
        }
        catch (ArgumentException aex)
        {
            _log.LogWarning("[{Class}.{Method}] Validation failed: {Message}", nameof(NewsFunctions), methodName, aex.Message);
            return new BadRequestObjectResult(new { error = aex.Message });
        }
        catch (KeyNotFoundException)
        {
            return new NotFoundResult();
        }
        catch (UnauthorizedAccessException uaex)
        {
            _log.LogWarning("[{Class}.{Method}] Forbidden: {Message}", nameof(NewsFunctions), methodName, uaex.Message);
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", nameof(NewsFunctions), methodName);
            throw;
        }
    }
}
