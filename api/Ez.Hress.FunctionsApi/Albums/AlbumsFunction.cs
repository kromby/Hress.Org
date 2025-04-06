using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ez.Hress.Albums.UseCases;
using Ez.Hress.Shared.UseCases;
using System.Text.Json;
using System.IO;
using Ez.Hress.Albums.Entities;
using System;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Albums;

public class CreateAlbumRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
}

public class AddImageToAlbumRequest
{
    public int ImageId { get; set; }
}

public class AlbumsFunction
{
    private readonly AlbumInteractor _albumInteractor;
    private readonly AuthenticationInteractor _authenticationInteractor;
    private readonly ILogger<AlbumsFunction> _log;

    public AlbumsFunction(AuthenticationInteractor authenticationInteractor, AlbumInteractor albumInteractor, ILogger<AlbumsFunction> log)
    {
        _authenticationInteractor = authenticationInteractor;
        _albumInteractor = albumInteractor;
        _log = log;
    }

    [Function("albums")]
    public async Task<IActionResult> RunAlbums(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "albums/{id:int?}")] HttpRequest req,
        int? id)
    {
        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAlbums));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[RunMagic] JWT is not valid!");
            return new UnauthorizedResult();
        }

        if (id.HasValue)
        {
            var entity = await _albumInteractor.GetAlbumAsync(id.Value);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(entity);
        }

        var list = await _albumInteractor.GetAlbumsAsync();
        return new OkObjectResult(list);
    }

    [Function("albumImages")]
    public async Task<IActionResult> RunAlbumImages(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "albums/{albumID:int}/images")] HttpRequest req,
        int albumID)
    {
        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAlbumImages));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[RunMagic] JWT is not valid!");
            return new UnauthorizedResult();
        }

        var list = await _albumInteractor.GetImagesAsync(albumID);
        return new OkObjectResult(list);
    }

    [Function("albumPost")]
    public async Task<IActionResult> RunAlbumCreate(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "albums")] HttpRequest req)
    {
        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAlbumCreate));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[CreateAlbum] JWT is not valid!");
            return new UnauthorizedResult();
        }

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<CreateAlbumRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null)
                return new BadRequestResult();

            var album = new Album(0, request.Name, request.Description, 0, request.Date ?? DateTime.UtcNow);
            var createdAlbum = await _albumInteractor.CreateAlbumAsync(album, userID);
            return new CreatedResult($"/api/albums/{createdAlbum.ID}", createdAlbum);
        }
        catch (JsonException)
        {
            return new BadRequestResult();
        }
        catch (ArgumentException ex)
        {
            _log.LogWarning("[CreateAlbum] Validation failed: {Message}", ex.Message);
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    [Function("albumImagePost")]
    public async Task<IActionResult> RunAlbumImagePost(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "albums/{albumId:int}/images")] HttpRequest req,
        int albumId)
    {
        _log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAlbumImagePost));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, _log, out int userID);
        if (!isJWTValid)
        {
            _log.LogInformation("[AddImageToAlbum] JWT is not valid!");
            return new UnauthorizedResult();
        }

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<AddImageToAlbumRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null)
                return new BadRequestResult();

            await _albumInteractor.AddImageToAlbumAsync(albumId, request.ImageId, userID);
            return new NoContentResult();
        }
        catch (JsonException)
        {
            return new BadRequestResult();
        }
        catch (ArgumentException ex)
        {
            _log.LogWarning("[AddImageToAlbum] Validation failed: {Message}", ex.Message);
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
