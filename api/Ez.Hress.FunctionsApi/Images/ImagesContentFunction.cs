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

public class ImagesContentFunction
{
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly ImageInteractor _imageInteractor;
    private readonly ILogger<ImagesContentFunction> _log;

    public ImagesContentFunction(AuthenticationInteractor authenticationInteractor, ImageInteractor imageInteractor, ILogger<ImagesContentFunction> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _imageInteractor = imageInteractor;
        _log = log;
    }

    [Function("images")]
    public async Task<IActionResult> RunImages(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "images")] HttpRequest req)
    {
        var methodName = nameof(RunImages);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(ImagesContentFunction), methodName);

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunImages));
            return new UnauthorizedResult();
        }

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var image = JsonConvert.DeserializeObject<ImageEntity>(requestBody);

        try
        {
            var entity = await _imageInteractor.SaveAsync(image, userID);
            return new OkObjectResult(entity);
        }
        catch(ArgumentException aex)
        {
            _log.LogError(aex, requestBody);
            return new BadRequestResult();
        }
    }

        [Function("imagesContent")]
    public async Task<IActionResult> RunImagesContent(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "images/{id:int}/content")] HttpRequest req,
        int id)
    {
        var methodName = nameof(RunImagesContent);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(ImagesContentFunction), methodName);

        int width = 0;
        if (!string.IsNullOrWhiteSpace(req.Query["thumb"]))
        {
            _ = bool.TryParse(req.Query["thumb"], out bool isThumb);
            width = isThumb ? 250 : 0;
        }

        if (!string.IsNullOrWhiteSpace(req.Query["width"]))
        {
            _ = int.TryParse(req.Query["width"], out width);
        }

        int height = 0;
        if (!string.IsNullOrWhiteSpace(req.Query["height"]))
        {
            _ = int.TryParse(req.Query["height"], out height);
        }

        var entity = await _imageInteractor.GetContentAsync(id, width, height);

        if (entity?.Content == null)
        {
            return new NotFoundResult();
        }

        req.HttpContext.Response.Headers.ContentDisposition  = $"inline; filename={entity.ID}.jpg";
        return new FileContentResult(entity.Content, new MediaTypeHeaderValue("image/jpeg"));
    }
}
