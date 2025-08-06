using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Images;

public class VideosContentFunction
{
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly VideoInteractor _videoInteractor;
    private readonly ILogger<VideosContentFunction> _log;

    public VideosContentFunction(AuthenticationInteractor authenticationInteractor, VideoInteractor videoInteractor, ILogger<VideosContentFunction> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _videoInteractor = videoInteractor;
        _log = log;
    }

        [Function("videosContent")]
    public async Task<IActionResult> RunVideosContent(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "videos/{id:guid}/content")] HttpRequest req,
        Guid id)
    {
        var methodName = nameof(RunVideosContent);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(VideosContentFunction), methodName);

        var entity = await _videoInteractor.GetContentAsync(id);

        if (entity?.Content == null)
        {
            return new NotFoundResult();
        }

        req.HttpContext.Response.Headers.ContentDisposition  = $"inline; filename={entity.ID}.jpg";
        return new FileContentResult(entity.Content, new MediaTypeHeaderValue("video/mp4"));
    }
}
