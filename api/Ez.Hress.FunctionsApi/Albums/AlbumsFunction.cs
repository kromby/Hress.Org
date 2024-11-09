using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ez.Hress.Albums.UseCases;
using Ez.Hress.Shared.UseCases;
using System.Text.Json;
using System.IO;
using Ez.Hress.Albums.Entities;
using System;

namespace Ez.Hress.FunctionsApi.Albums;

public class CreateAlbumRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
}

public class AlbumsFunction
{
    private readonly AlbumInteractor _albumInteractor;
    private readonly AuthenticationInteractor _authenticationInteractor;
    public AlbumsFunction(AuthenticationInteractor authenticationInteractor, AlbumInteractor albumInteractor)
    {
        _authenticationInteractor = authenticationInteractor;
        _albumInteractor = albumInteractor;
    }

    [FunctionName("albums")]
    public async Task<IActionResult> RunAlbums(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "albums/{id:int?}")] HttpRequest req,
        int? id, ILogger log)
    {
        log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAlbums));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);
        if (!isJWTValid)
        {
            log.LogInformation("[RunMagic] JWT is not valid!");
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

    [FunctionName("albumImages")]
    public async Task<IActionResult> RunAlbumImages(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "albums/{albumID:int}/images")] HttpRequest req,
        int albumID, ILogger log)
    {
        log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAlbumImages));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);
        if (!isJWTValid)
        {
            log.LogInformation("[RunMagic] JWT is not valid!");
            return new UnauthorizedResult();
        }

        var list = await _albumInteractor.GetImagesAsync(albumID);
        return new OkObjectResult(list);
    }

    [FunctionName("albumPost")]
    public async Task<IActionResult> RunAlbumCreate(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "albums")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAlbumCreate));

        var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);
        if (!isJWTValid)
        {
            log.LogInformation("[CreateAlbum] JWT is not valid!");
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
            log.LogWarning("[CreateAlbum] Validation failed: {Message}", ex.Message);
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
